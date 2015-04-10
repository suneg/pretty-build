using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pretty.Build
{
    public class PrettyParser
    {
        private string currentSection;

        public PrettyParser()
        {

        }

        public Project Parse(string projectSpecification, Project result, String specFilePath)
        {
            var lines = projectSpecification.Split('\n');

            foreach (var line in lines)
            {
                if (line.Trim().Length == 0)
                    continue;

                if (line.Trim().EndsWith(":"))
                {
                    currentSection = line.Trim().Replace(":", "").ToLower();
                }
                else if (char.IsLetter(line[0]))
                {

                    if (line.ToLower().StartsWith("name:"))
                    {
                        result.Name = line.Split(':')[1].Trim();
                    }
                    else if (line.ToLower().StartsWith("type:"))
                    {
                        String configuredValue = line.Split(':')[1].Trim();
                        var commentStart = configuredValue.IndexOf("#");
                        if (commentStart > -1) 
                        {
                            configuredValue = configuredValue.Substring(0, commentStart).Trim();
                        }

                        try
                        {
                            result.Type = (ProjectType)Enum.Parse(typeof(ProjectType), configuredValue, true);
                        }
                        catch (ArgumentException ex)
                        {
                            var validValues = new List<String>();
                            foreach (var value in Enum.GetValues(typeof(ProjectType)).Cast<ProjectType>())
                            {
                                validValues.Add(value.ToString());
                            }

                            throw new InvalidConfigurationException("Type", configuredValue, String.Join(", ", validValues.ToArray()), specFilePath);
                        }
                    }
                    else if (line.ToLower().StartsWith("output:"))
                    {
                        result.Output = line.Split(':')[1].Trim();
                    }
                }

                if (line.StartsWith("    ") || line.StartsWith("  "))
                {
                    if (currentSection == "dependencies")
                    {
                        result.Dependencies.Add(line.Trim());
                    }
                    else if (currentSection == "packages")
                    {
                        var req = line.Trim().Split(':');
                        var dic = new Dictionary<String, String>();
                        dic.Add(req[0].Trim(), req[1].Trim());
                        result.Packages.Add(dic);
                    }
                }
            }

            // Add defaults
            if(result.Output == null) 
            {
                var extension = "";

                switch(result.Type) 
                {
                    case ProjectType.Executable:
                        extension = ".exe";
                        break;
                    case ProjectType.Library:
                        extension = ".dll";
                        break;
                    default:
                        extension = ".dll";
                        break;
                }

                result.Output = result.Name + extension;
            }

            return result;
        }

        public Project Parse(FileInfo file, Project result)
        {
            String spec = System.IO.File.ReadAllText(file.FullName);
            return Parse(spec, result, file.FullName);
            
        }
    }
}
