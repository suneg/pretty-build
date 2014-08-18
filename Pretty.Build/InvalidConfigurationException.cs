using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Pretty.Build
{
    public class InvalidConfigurationException : Exception
    {
        private string key;
        private string value;
        private string explanation;
        private string projectPath;

        public InvalidConfigurationException(string key, string value, string explanation, string projectPath)
        {
            this.key = key;
            this.value = value;
            this.explanation = explanation;
            this.projectPath = projectPath;
        }

        public override string ToString()
        {
            ;

            var shortPath = projectPath.Substring(Directory.GetCurrentDirectory().Length+1);

            return String.Format("In {3}:\n    {0} is incorrectly configured: '{1}'. Valid values are [{2}]", key, value, explanation, shortPath);
        }
    }
}
