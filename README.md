# Pretty Build #

An experiment with a back-to-basics build tool for .NET


Goal 1: Replace 100 lines of non-readable csproj-files with 3 lines of easy-to-understand plain-text

Goal 2: Avoid cross-project references defined in csproj files

Goal 3: Avoid 30 lines of MSBuild nonsense build-output

Goal 4: Avoid the excessive copying of binaries during build (defaut MSBuild behavior)

Goal 5: Seperate compile & test from packaging and deployment

Goal 6: Remove configuration (*.config) from build output

Goal 7: Use output coloring to signify default (implicit) versus explicit settings


### A pretty project file ###


```
C:\repos\MyAwesomeProject>type project.txt
Name: MyAwesomeProject
Type: library

Dependencies:
    MyProject.Common

Requires:
    NUnit : 2.6.3

Sources:
    **\*.cs
```

..still too verbose? Why not rely on defaults to make it simpler?

```
C:\repos\MyAwesomeProject>type project.txt
Dependencies:
    MyProject.Common

Requires:
    NUnit : 2.6.3
```

..still too verbose? If you project has no external dependencies (third-party or custom) you can actually build using an empty file!

```
C:\repos\MyAwesomeProject>type project.txt
C:\repos\MyAwesomeProject>
```

..Now we're talking!

### What about the output? ###

Here's what you are probably looking at today.
![Typical MSBuild output](raw.github.com/suneg/pretty-build/master/doc/msbuild.png)

Here's the same project build with Pretty Build.
![Human readable output](raw.github.com/suneg/pretty-build/master/doc/pretty.png)

(Wonder where the speed difference comes from by the way...)
