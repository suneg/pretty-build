﻿using System;
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

        public Project Parse(FileInfo file, Project result)
        {
            String[] lines = System.IO.File.ReadAllLines(file.FullName);

            foreach(var line in lines) {
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
                        result.Type = (ProjectType)Enum.Parse(typeof(ProjectType), line.Split(':')[1].Trim(), true);
                    }
                    if(line.ToLower().StartsWith("output:"))
                    {
                        result.Output = line.Split(':')[1].Trim();
                    }
                }

                if (line.StartsWith("    "))
                {
                    if (currentSection == "dependencies")
                    {
                        result.Dependencies.Add(line.Trim());
                    } 
                    else if (currentSection == "requires")
                    {
                        var req = line.Trim().Split('-');
                        var dic = new Dictionary<String, String>();
                        dic.Add(req[0].Trim(), req[1].Trim());
                        result.Requires.Add(dic);
                    }
                }
            }

            return result;
        }
    }
}