using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pretty.Build
{
    public class Project
    {

        private string name;
        private ProjectType type;

        public ProjectType Type {
            get { return type; }
            set { type = value; 
                if(type == ProjectType.Executable) 
                {
                    Output = Output.Replace(".dll", ".exe");
                }
            } 
        }
        public String Output { get; set; }
        public List<Dictionary<String, String>> Requires = new List<Dictionary<String, String>>();
        public String Name {
            get { return name; }
            set { 
                name = value;

                var outputDirectory = System.Environment.ExpandEnvironmentVariables(String.Format(@"%userprofile%\.pretty\cache\{0}\lib", name));
                Directory.CreateDirectory(outputDirectory);

                OutputPath = outputDirectory;
            }
        }
        public List<String> Dependencies = new List<String>();
        public List<String> Sources = new List<String>();
        public System.IO.DirectoryInfo Path { get ; set; }
        public String OutputPath { get; set; }
    }
}
