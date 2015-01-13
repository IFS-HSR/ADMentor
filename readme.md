# AD Mentor

## Project Setup

1. Register the ADMentor Add-In in EA by executing `ADMentorAddIn.reg`
2. Start Visual Studio as administrator
3. Import the solution into Visual Studio
4. Make sure ADMentor is the solutions "Startup project"
4. Enable support for the "Start Debugging" command in VS:
  1. Open project properties of the ADMentor project (Right click on project -> Properties)
  2. Open Tab "Debug"
  3. Check if the "Start external programm" option is checked and select the path to the Enterprise Architect executable
  4. Invoke the "Start" command in VS; Enterprise Architect should start with the AD Add-In loaded