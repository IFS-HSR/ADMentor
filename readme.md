# AD Mentor

## Project Setup

1. Run the following command on the command line to register the add-in:
   reg add "HKEY_CURRENT_USER\Software\Sparx Systems\EAAddins\ADAddIn" /ve /d "ADAddIn.ADAddIn"
2. Start Visual Studio as administrator
3. Import the solution into Visual Studio
4. Make sure AdAddIn is the solutions "Startup project"
4. Enable support for the "Start Debugging" command in VS:
  1. Open project properties of the ADAddIn project (Right click on project -> Properties)
  2. Open Tab "Debug"
  3. Check if the "Start external programm" option is checked and select the path to the Enterprise Architect executable
  4. Invoke the "Start" command in VS; Enterprise Architect should start with the AD Add-In loaded
5. Enable logging to the VS console:
  1. Copy "ADAddIn\NLog.config" into EA's programm folder
  2. Start EA; AD log messages should now appear in the VS Output tab