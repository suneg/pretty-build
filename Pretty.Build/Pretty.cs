using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using NDesk.Options;
using Pretty.Build.Model;


namespace Pretty.Build
{
    
    public class Pretty
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public enum PrettyCommand { None = 0, Build = 1, Clean = 2, Test = 4 }
        
        public static bool help = false;
        public static bool verbose = false;

        public static List<BuildResult> result = new List<BuildResult>();

        public static void Main(String[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e)
                => DisplayUnhandledException(e.ExceptionObject);

            PrettyCommand command = PrettyCommand.None;

            var p = new OptionSet () {
                { "b|build",    v => command |= PrettyCommand.Build },
                { "c|clean",    v => command |= PrettyCommand.Clean },
                { "t|test",     v => command |= PrettyCommand.Test },
                { "v|verbose", v => verbose = true },
                { "h|?|help",   v => help = v != null },
            };

            
            Console.ForegroundColor = ConsoleColor.White;

            var extra = p.Parse(args);

            if (help)
            {
                Console.WriteLine("Usage: pretty [OPTIONS]");
                Console.WriteLine("A simple build alternative for .NET");
                Console.WriteLine();
                Console.WriteLine("Options:");

                p.WriteOptionDescriptions(Console.Out);
                return;
            }


            String defaultFile = "project.txt";

            if (extra.Count == 0 && !File.Exists(defaultFile))
            {
                Write("Project file 'project.txt' is not found. Create it now? Y/N: ", ConsoleColor.Yellow);
                String input = Console.ReadLine();
                
                if (input.ToLower() == "y")
                {
                    File.Create("project.txt").Close();
                }
                else
                {
                    return;
                }
                
            }
            
            FileInfo projectFile = new FileInfo(extra.Count > 0 ? extra[0] : defaultFile);

            Project project = new Project();

            initializeDefaults(project, projectFile);

            var parser = new PrettyParser();
            try
            {
                project = parser.Parse(projectFile, project);
            }
            catch (InvalidConfigurationException ex)
            {
                WriteLine(ex.ToString(), ConsoleColor.Yellow);
                return;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString(), ConsoleColor.Yellow);
                return;
            }
            
            
            Write("Name: ", ConsoleColor.White);
            WriteLine(project.Name, ConsoleColor.Gray);

            Write("Type: ", ConsoleColor.White);
            WriteLine(project.Type.ToString().ToLower(), ConsoleColor.Gray);
            Console.WriteLine();

            WriteLine("Dependencies: ", ConsoleColor.White);
            
            project.Dependencies.ForEach(i => WriteLine(String.Format("    {0}", i), ConsoleColor.Cyan));
            if (project.Dependencies.Count == 0)
            {
                WriteLine("  # None", ConsoleColor.DarkGray);
            }
            
            Console.WriteLine();

            WriteLine("Packages: ", ConsoleColor.White);

            foreach(var requirement in project.Packages) {
                var requirementDirectoryName = String.Format("{0}.{1}", requirement.Keys.First<String>(), requirement.Values.First<String>());
                
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var requirementPath = Path.Combine(home, String.Join(Path.DirectorySeparatorChar.ToString(), ".pretty", "cache", requirementDirectoryName, "lib"));

                if (Directory.Exists(requirementPath))
                {
                    WriteLine(
                        String.Format("    {0} : {1}", 
                        requirement.Keys.First<String>(), 
                        requirement.Values.First<String>())
                        , ConsoleColor.Cyan);
                }
                else
                {
                    WriteLine(
                        String.Format("    {0} : {1}  # X Missing package? Hint: try 'nuget install {2} -Version {1}'", 
                        requirement.Keys.First<String>(), 
                        requirement.Values.First<String>(), 
                        requirement.Keys.First<String>().ToLower())
                        , ConsoleColor.Red);
                }
            }

            if (project.Packages.Count == 0)
            {
                WriteLine("  # None", ConsoleColor.DarkGray);
            }
            
            
            Console.WriteLine();

            AttachAllSourceCode(project, projectFile.Directory);

            if (command == PrettyCommand.None)
            {
                result.Add(new BuildResult("Spec: VALID" , 0, ConsoleColor.Green));
            }
                

            if ((command & PrettyCommand.Clean) == PrettyCommand.Clean)
            {
                var start = DateTime.Now;
                var files = new List<String>();
                files.Add(Path.Combine(project.OutputPath, project.Output));
                files.Add(Path.Combine(project.OutputPath, project.Output.Replace(".dll", ".pdb")));

                foreach(var file in files) {
                    
                    if(File.Exists(file)) 
                    {
                        File.Delete(file);

                        if (verbose)
                        {
                            WriteLine("Deleted " + file, ConsoleColor.DarkGray);
                        }
                    }
                }

                result.Add(new BuildResult("Clean: SUCCESS", DateTime.Now.Subtract(start).TotalSeconds, ConsoleColor.Green));
            }

            if ((command & PrettyCommand.Build) == PrettyCommand.Build)
            {
                Compile(project);
            }

            if ((command & PrettyCommand.Test) == PrettyCommand.Test)
            {
                result.Add(new BuildResult("Test : SUCCESS", 1.9, ConsoleColor.Green));
            } 

            Console.WriteLine();
            // Print result
            foreach(var element in result)
            {
                WriteLine(
                    String.Format("{0} ({1}s)", element.Text, Math.Round(element.Seconds, 2))
                    , element.Color);
            }
        }

        static void DisplayUnhandledException(object exceptionObject)
        {
            var e = exceptionObject as Exception;
            if (e == null)
            {
                e = new NotSupportedException("Unhandled exception is not an exception type: "
                   + exceptionObject.ToString()
                );
            }
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("failure. exiting", (Exception)e.ExceptionObject);

            System.Environment.Exit(0);
        }

        private static void AttachAllSourceCode(Project project, DirectoryInfo directoryInfo)
        {
            WriteLine("Sources: ", ConsoleColor.White);

            WriteLine("    **\\*.cs", ConsoleColor.DarkGray);
            foreach (FileInfo file in directoryInfo.EnumerateFiles("*.cs", SearchOption.AllDirectories))
            {
                project.Sources.Add(file.FullName);
            }
            Console.WriteLine();
        }

        private static string ShortFileName(String filePath, Project project)
        {
            return filePath.Substring(project.Path.FullName.Length + 1);
        }

        private static void Compile(Project project)
        {
            var outputAssembly = Path.Combine(project.OutputPath, project.Output);
            var skip = false;
            var failed = false;

            if (File.Exists(outputAssembly))
            {
                if(GetLatestSourceModification(project).CompareTo(File.GetLastWriteTime(outputAssembly)) < 0) {
                    Console.WriteLine("Already at latest -> Skipping");
                    skip = true;
                }
            }

            var start = DateTime.Now;

            if (!skip)
            {
                CSharpCodeProvider codeProvider = new CSharpCodeProvider();

                System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
                parameters.ReferencedAssemblies.Add("System.dll");
                parameters.ReferencedAssemblies.Add("System.Core.dll");
                parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
                parameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
                parameters.ReferencedAssemblies.Add("System.Data.dll");
                parameters.ReferencedAssemblies.Add("System.Xml.dll");
                parameters.ReferencedAssemblies.Add("System.Configuration.dll");
   
                foreach (var requirement in project.Packages)
                {
                    var requirementDirectoryName = String.Format("{0}.{1}", requirement.Keys.First<String>(), requirement.Values.First<String>());
                    
                    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    var requirementPath = Path.Combine(home, String.Join(Path.DirectorySeparatorChar.ToString(), ".pretty", "cache", requirementDirectoryName, "lib"));

                    var searchOrder = new string[]
                    {
                        Path.Combine(requirementPath, "net45-full"),
                        Path.Combine(requirementPath, "net45"),
                        Path.Combine(requirementPath, "net40-full"),
                        Path.Combine(requirementPath, "net40"),
                        Path.Combine(requirementPath, "net35"),
                        requirementPath
                    };
                


                    // TODO: No support for different .NET versions (3.5 / 4.0 / 4.5)
                    bool found = false;
                    for (int i = 0; i < searchOrder.Length; i++)
                    {
                        if (verbose)
                        {
                            WriteLine("Searching " + searchOrder[i] + "..", ConsoleColor.DarkGray);
                        }
                        if (Directory.Exists(searchOrder[i]))
                        {
                            found = true;
                            
                            var assemblies = Directory.GetFiles(searchOrder[i], "*.dll");
                            assemblies.All(v =>
                            {
                                return true;
                            });

                            if (assemblies.Length == 0)
                            {
                                Console.WriteLine("No assemblies found in '" + searchOrder[i] + "'");
                            }
                            else
                            {
                                foreach (var assembly in assemblies)
                                {
                                    if (verbose)
                                    {
                                        WriteLine("Adding assembly: " + assembly, ConsoleColor.DarkGray);
                                    }

                                    log.Info("Adding assembly: " + assembly);
                                }
                            }


                            parameters.ReferencedAssemblies.AddRange(assemblies);

                            break;
                        }
                    }

                    if (!found)
                    {
                        WriteLine(
                        String.Format("    {0} : {1}  # Missing package? Hint: try 'nuget install {2} -Version {1}'",
                        requirement.Keys.First<String>(),
                        requirement.Values.First<String>(),
                        requirement.Keys.First<String>().ToLower())
                        , ConsoleColor.Red);
                        
                    }
                }

                foreach (var dependency in project.Dependencies)
                {
                    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);


                    var dependencyPath = Path.Combine(home, String.Join(Path.DirectorySeparatorChar.ToString(), ".pretty", "cache", dependency, "lib"));

                    WriteLine("Path: " + dependencyPath, ConsoleColor.Yellow);

                    //var dependencyPath = System.Environment.ExpandEnvironmentVariables(String.Format(@"%userprofile%\.pretty\cache\{0}\lib", dependency));

                    // TODO: No support for different .NET versions (3.5 / 4.0 / 4.5)
                    var assemblies = Directory.GetFiles(dependencyPath, "*.dll");
                    parameters.ReferencedAssemblies.AddRange(assemblies);
                }

                parameters.GenerateExecutable = project.Type == ProjectType.Executable;

                parameters.OutputAssembly = outputAssembly;
                parameters.IncludeDebugInformation = true;
                //parameters.CompilerOptions = "/highentropyva+ /debug+ /debug:full /optimize- /utf8output /noconfig /subsystemversion:6.00";
                parameters.CompilerOptions = "/debug+ /debug:full /optimize- /noconfig";

                CompilerResults results = codeProvider.CompileAssemblyFromFile(parameters, project.Sources.ToArray());
                if (results.Errors.Count > 0)
                {
                    WriteLine("Errors:", ConsoleColor.White);
                    Console.ForegroundColor = ConsoleColor.Yellow;

                    int i = 1;
                    foreach (CompilerError CompErr in results.Errors)
                    {

                        if (CompErr.FileName.Length > 0)
                        {
                            Console.WriteLine("  {0, 2}. {1}:{2} ({3}) {4}\n", i, ShortFileName(CompErr.FileName, project), CompErr.Line,
                                CompErr.ErrorNumber, CompErr.ErrorText);
                        }
                        else
                        {
                            //Console.WriteLine(i + "    (" + CompErr.ErrorNumber + ") "
                            //    + CompErr.ErrorText + Environment.NewLine + Environment.NewLine);
                            Console.WriteLine("  {0, 2}. ({1}) {2}\n", i,
                               CompErr.ErrorNumber, CompErr.ErrorText);
                        }

                        i++;
                    }
                    Console.ResetColor();

                    result.Add(new BuildResult("Build: FAILURE", DateTime.Now.Subtract(start).TotalSeconds, ConsoleColor.Red));

                    failed = true;
                }
            }
            
            if (!failed)
            {
                if (verbose)
                {
                    WriteLine("Created " + outputAssembly, ConsoleColor.DarkGray); 
                }
                    
                result.Add(new BuildResult("Build: SUCCESS", DateTime.Now.Subtract(start).TotalSeconds, ConsoleColor.Green));
            }
        }

        private static DateTime GetLatestSourceModification(Project project)
        {
            DateTime? latestModificationDate = null;

            foreach (var file in project.Sources)
            {
                var lastWriteTime = File.GetLastWriteTime(file);

                if (lastWriteTime.CompareTo(latestModificationDate) > 0)
                {
                    latestModificationDate = lastWriteTime;
                }
            }

            return latestModificationDate != null ? (DateTime)latestModificationDate : DateTime.Now;
        }

        private static void initializeDefaults(Project project, FileInfo projectFile)
        {
            project.Name = projectFile.Directory.Name;
            project.Path = projectFile.Directory;

            var outputDirectory = "bin"; //System.Environment.ExpandEnvironmentVariables(String.Format(@"%userprofile%\.pretty\cache\{0}\lib", project.Name));
            Directory.CreateDirectory(outputDirectory);

            project.OutputPath = outputDirectory;
        }

        private static void Write(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ResetColor();
        }

        private static void WriteLine(string message, ConsoleColor color)
        {
            Write(message, color);
            Console.WriteLine();
        }
    }
}
