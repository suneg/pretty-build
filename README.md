# Pretty Build #

Eksperiment med en alternativ build løsning til .NET


Ide 1: Skift 100 liniers non-readable csproj-filer ud med 3 liniers letforståelig plain-text

Ide 2: Undgå cross-project referencer defineret i csproj filer

Ide 3: Undgå 30 liniers build-output nonsens fra MSBuild

Ide 4: Undgå omfattende kopiering af binaries under build ved at gemme output et andet sted end standard .\bin\debug

Ide 5: Skil compile+test fra packaging og deployment

Ide 6: Fjern configuration (*.config) fra build output


### Første udkast til "Minimum viable build file" ###


```
#!text
# cat project.txt
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
# cat project.txt
Dependencies:
    BestBrains.System

Requires:
    NUnit : 2.6.3
```

..stadig for stor. Hvad nu hvis man ikke havde brugt NUnit og BestBrains.System (en plausibel start på et projekt)

```
#!text
# cat project.txt
(tom)
```

..Now we're talking!

### Kald til MSBuild ###

Idag:
![Screenshot 2014-05-30 00.44.21.png](https://bitbucket.org/repo/7nKE86/images/1445401539-Screenshot%202014-05-30%2000.44.21.png)

I fremtiden:
![Screenshot 2014-06-01 23.52.32.png](https://bitbucket.org/repo/7nKE86/images/1396722961-Screenshot%202014-06-01%2023.52.32.png)