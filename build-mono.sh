#!/bin/sh

mkdir -p Pretty.Build/bin
mcs -fullpaths /optimize- /debug+ /debug:full -platform:anycpu -target:exe \
    -out:Pretty.Build/bin/pretty.exe \
    -r:packages/NDesk.Options.0.2.1/lib/NDesk.Options.dll \
    -r:packages/log4net.2.0.3/lib/net40-full/log4net.dll \
    -r:packages/SharpZipLib.0.86.0/lib/20/ICSharpCode.SharpZipLib.dll \
    Pretty.Build/BuildSpec.cs Pretty.Build/Pretty.cs Pretty.Build/PrettyParser.cs \
    Pretty.Build/ProjectType.cs Pretty.Build/Properties/AssemblyInfo.cs \
    Pretty.Build/Model/BuildResult.cs Pretty.Build/Model/Project.cs \
    Pretty.Build/InvalidConfigurationException.cs
