using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pretty.Build.Model
{
    public class BuildResult
    {
        public String Text { get; set; }
        public double Seconds { get; set; }
        public ConsoleColor Color { get; set; }

        public BuildResult(String text, double seconds, ConsoleColor color)
        {
            this.Text = text;
            this.Seconds = seconds;
            this.Color = color;
        }
    }
}
