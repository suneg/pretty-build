using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pretty.Build.Test
{
    [TestFixture]
    public class PrettyParserTest
    {
        [Test]
        public void TestBasicSpec()
        {
            var project = @"
Name: MyProject
Type: library

Packages:
  NUnit : 2.6.3
  NDesk.Options : 4.5.9

Dependencies:
  MyCommon.Library
";
            Project result = new Project();

            var parser = new PrettyParser();
            parser.Parse(project, result, null);

            Assert.AreEqual("MyProject", result.Name);
            Assert.AreEqual(ProjectType.Library, result.Type);

            Assert.AreEqual(2, result.Packages.Count);
            Assert.AreEqual("2.6.3", result.Packages[0]["NUnit"]);
            Assert.AreEqual("4.5.9", result.Packages[1]["NDesk.Options"]);
            
            Assert.AreEqual(1, result.Dependencies.Count);
            Assert.AreEqual("MyCommon.Library", result.Dependencies[0]);

            Assert.AreEqual("MyProject.dll", result.Output);
        }

        [Test]
        public void TestComments()
        {
            var project = @"
# just a comment
Name: MyProject

# no effect
Type: executable #hopefully

Output: CrazyName.exe
# the end
";

            Project result = new Project();

            var parser = new PrettyParser();
            parser.Parse(project, result, null);

            Assert.AreEqual("MyProject", result.Name);
            Assert.AreEqual(ProjectType.Executable, result.Type);
            Assert.AreEqual("CrazyName.exe", result.Output);
        }

        [Test]
        public void TestDefaultOutputFileName()
        {
            var project = @"
Name: MyProject
Type: executable
";

            Project result = new Project();

            var parser = new PrettyParser();
            parser.Parse(project, result, null);

            Assert.AreEqual("MyProject.exe", result.Output);
        }
        
        [Test]
        public void TestDefaultOutputType()
        {
            var project = @"
Name: MyProject
";

            Project result = new Project();

            var parser = new PrettyParser();
            parser.Parse(project, result, null);

            Assert.AreEqual(ProjectType.Library, result.Type);
            Assert.AreEqual("MyProject.dll", result.Output);
        }

    }
}
