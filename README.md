# Pretty Build #

An experiment with a back-to-basics build tool for .NET


Goal 1: Replace 100 lines of non-readable csproj-files with 3 lines of easy-to-understand plain-text

Goal 2: Avoid cross-project references defined in csproj files

Goal 3: Avoid 30 lines of MSBuild nonsense build-output

Goal 4: Avoid the excessive copying of binaries during build (defaut MSBuild behavior)

Goal 5: Seperate compile & test from packaging and deployment

Goal 6: Remove configuration (*.config) from build output

Goal 7: Use coloring to default (implicit) versus explicit settings


### First draft of a "Minimum viable build file" ###


```
C:\repos\MyAwesomeProject>type project.txt
Name: MyAwesomeProject
Type: library

Dependencies:
    MyProject.Common

Requires:
    NUnit : 2.6.3

Sources:
    Class1.cs
    Properties\AssemblyInfo.cs
```

..still too verbose. Lets remove the required Name parameter and use the directory name as default value. Also lets set the default project type to Library (standard .NET assembly). Also we'll remove the list of source files, and include all *.cs files recursively by default

```
C:\repos\MyAwesomeProject>type project.txt
Dependencies:
    MyProject.Common

Requires:
    NUnit : 2.6.3
```

..still too verbose. What if we had not included NUnit or the MyProject.Common assembly? (a plausible start start of a new .NET project)

```
C:\repos\MyAwesomeProject>type project.txt
C:\repos\MyAwesomeProject>
```

..Now we're talking!

### Call to MSBuild ###

Today:
![](raw.github.com/suneg/pretty-build/master/doc/msbuild.png)

In the future:
![](raw.github.com/suneg/pretty-build/master/doc/pretty.png)

(wonder where the speed difference comes from)
