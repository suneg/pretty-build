# Pretty Build #

Eksperiment med en alternativ build løsning til .NET


Ide 1: Skift 100 liniers non-readable csproj-filer ud med 3 liniers letforståelig plain-text
Ide 2: Undgå cross-project referencer defineret i csproj filer
Ide 3: Undgå 30 liniers build-output nonsens fra MSBuild
Ide 4: Undgå omfattende kopiering af binaries under build ved at gemme output et andet sted end standard .\bin\debug


### Første udkast til "Minimum viable build file" ###

```
#!text
Name: MyAwesomeProject
Type: library

Dependencies:
    BestBrains.System

Requires:
    NUnit : 2.6.3

Sources:
   Class1.cs
   Properties\AssemblyInfo.cs
```

..stadig for stor. Vi fjerner krav til Name parameteren og defaulter til directory navnet. Vi defaulter Type til Library (standard assembly). Vi fjerner source listen og defaulter *.cs i alle subdirs

```
#!text
Dependencies:
    BestBrains.System

Requires:
    NUnit : 2.6.3
```
