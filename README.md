# Pretty Build #

Eksperiment med en alternativ build løsning til .NET


Ide 1: Skift 100 liniers non-readable csproj-filer ud med 3 liniers letforståelig plain-text
Ide 2: Undgå cross-project referencer defineret i csproj filer
Ide 3: Undgå 30 liniers build-output nonsens fra MSBuild
Ide 4: Undgå omfattende kopiering af binaries under build ved at gemme output et andet sted end standard .\bin\debug