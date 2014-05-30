using Newtonsoft.Json;
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
        public enum PrettyCommand { None = 0, Build = 1, Clean = 2 }
        
        public static bool help = false;
        public static bool verbose = false;
        public static bool onlyInfo = false;

        public static List<BuildResult> result = new List<BuildResult>();

        public static void Main(String[] args)
        {
            
            PrettyCommand command = PrettyCommand.None;

            var p = new OptionSet () {
                { "b|build",    v => command |= PrettyCommand.Build },
                { "c|clean",    v => command |= PrettyCommand.Clean },
                { "i|info",     v => onlyInfo = true },
                { "v|verbose", v => verbose = true },
                { "h|?|help",   v => help = v != null },
            };

            Console.ForegroundColor = ConsoleColor.White;

            p.Parse(args);

            if (help)
            {
                Console.WriteLine("Usage: pretty [OPTIONS]");
                Console.WriteLine("A simple build alternative for .NET");
                Console.WriteLine();
                Console.WriteLine("Options:");

                p.WriteOptionDescriptions(Console.Out);
                return;
            }
            

            FileInfo projectFile = new FileInfo("project.json");
            String json = System.IO.File.ReadAllText(projectFile.FullName, Encoding.UTF8);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            Project project = JsonConvert.DeserializeObject<Project>(json, settings);

            initializeDefaults(project, projectFile);

            
            Console.Write("Project: ");
            Console.WriteLine(project.Name);

            Console.WriteLine("Type: {0}", project.Type.ToString().ToLower());
            Console.ResetColor();
            //Console.WriteLine("Output: {0}", project.Output);

            Console.WriteLine(String.Empty);
            Console.WriteLine("Dependencies: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            project.Dependencies.ForEach(i => Console.WriteLine("    {0}", i));
            Console.ResetColor();

            Console.WriteLine(String.Empty);
            Console.WriteLine("Requires: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            project.Requires.ForEach(i => Console.WriteLine("    {0} - {1}", i.Keys.First<String>(), i.Values.First<String>()));
            Console.ResetColor();
            Console.WriteLine();

            AttachAllSourceCode(project, projectFile.Directory);

            if (onlyInfo)
                return;

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
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine("Deleted " + file);
                            Console.ResetColor();
                        }
                    }
                }

                result.Add(new BuildResult("Clean: SUCCESS", DateTime.Now.Subtract(start).TotalSeconds, ConsoleColor.Green));
            }

            if ((command & PrettyCommand.Build) == PrettyCommand.Build)
            {
                Compile(project);
            }


            Console.WriteLine();
            // Print result
            foreach(var element in result)
            {
                Console.ForegroundColor = element.Color;
                Console.WriteLine("{0} ({1}s)", element.Text, Math.Round(element.Seconds, 2));
                Console.ResetColor();
            }
        }

        private static void AttachAllSourceCode(Project project, DirectoryInfo directoryInfo)
        {
            Console.WriteLine("Sources:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            foreach (FileInfo file in directoryInfo.EnumerateFiles("*.cs", SearchOption.AllDirectories))
            {
                Console.WriteLine("    " + ShortFileName(file.FullName, project));
                project.Sources.Add(file.FullName);
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        private static string ShortFileName(String filePath, Project project)
        {
            return filePath.Substring(project.Path.FullName.Length + 1);
        }

        private static void Compile(Project project)
        {
            var start = DateTime.Now;
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();

            System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            parameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            parameters.ReferencedAssemblies.Add("System.Data.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");
            //parameters.ReferencedAssemblies.Add(@"C:\Users\Sune\workspace\old-svn-code\sandbox\orm-bestbrains\Frog.Orm\Frog.Orm.dll");

            foreach(var requirement in project.Requires) 
            {
                var requirementDirectoryName = String.Format("{0}.{1}", requirement.Keys.First<String>(), requirement.Values.First<String>());
                var requirementPath = System.Environment.ExpandEnvironmentVariables(String.Format(@"%userprofile%\.pretty\cache\{0}\lib", requirementDirectoryName));

                // TODO: No support for different .NET versions (3.5 / 4.0 / 4.5)
                var assemblies = Directory.GetFiles(requirementPath, "*.dll");
                parameters.ReferencedAssemblies.AddRange(assemblies);
            }

            foreach (var dependency in project.Dependencies)
            {
                var dependencyPath = System.Environment.ExpandEnvironmentVariables(String.Format(@"%userprofile%\.pretty\cache\{0}\lib", dependency));

                // TODO: No support for different .NET versions (3.5 / 4.0 / 4.5)
                var assemblies = Directory.GetFiles(dependencyPath, "*.dll");
                parameters.ReferencedAssemblies.AddRange(assemblies);
            }
            
            parameters.GenerateExecutable = project.Type == ProjectType.Executable;

            parameters.OutputAssembly = Path.Combine(project.OutputPath, project.Output); ;
            parameters.IncludeDebugInformation = true;
            //parameters.CompilerOptions = "/keyfile:..\\frog.snk";
            CompilerResults results = codeProvider.CompileAssemblyFromFile(parameters, project.Sources.ToArray());

            if (results.Errors.Count > 0)
            {
                Console.WriteLine("Errors:");
                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach (CompilerError CompErr in results.Errors)
                {

                    if (CompErr.FileName.Length > 0)
                    {
                        Console.WriteLine("    " + ShortFileName(CompErr.FileName, project) + ":" + CompErr.Line +
                            " (" + CompErr.ErrorNumber + ") "
                            + CompErr.ErrorText + Environment.NewLine + Environment.NewLine);
                    }
                    else
                    {
                        Console.WriteLine("    (" + CompErr.ErrorNumber + ") "
                            + CompErr.ErrorText + Environment.NewLine + Environment.NewLine);
                    }
                }
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Build: FAILURE ({0}s)", Math.Round(DateTime.Now.Subtract(start).TotalSeconds, 2));
                Console.ResetColor();
            }
            else
            {
                if (verbose)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("Created " + parameters.OutputAssembly);
                    Console.ResetColor();
                }
                    
                result.Add(new BuildResult("Build: SUCCESS", DateTime.Now.Subtract(start).TotalSeconds, ConsoleColor.Green));
            }
        }

        private static void initializeDefaults(Project project, FileInfo projectFile)
        {
            project.Name = projectFile.Directory.Name;
            project.Path = projectFile.Directory;
            project.Output = project.Name + ".dll";

            var outputDirectory = System.Environment.ExpandEnvironmentVariables(String.Format(@"%userprofile%\.pretty\cache\{0}\lib", project.Name));
            Directory.CreateDirectory(outputDirectory);

            project.OutputPath = outputDirectory;
        }
    }
}
