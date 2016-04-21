# Developer's Guide

## Prerequisites

* Visual Studio 2013
* [Visual Studio Installer Projects Extensions](https://visualstudiogallery.msdn.microsoft.com/9abe329c-9bba-44a1-be59-0fbf6151054d)
* Enterprise Architect 10 or newer

## Set up the Project

1. Checkout the ADMentor repository
1. Copy `Interop.EA.dll` from your local Enterprise Architect installation to the repository root folder
1. Start Visual Studio as _Administrator_
1. Open the ADMentor solution
1. Right Click on the ADMentor project and choose "Set as StartUp Project"
1. Open the properties of the ADMentor project and go to the "Debug" tab
1. Set the path to your EA installation in the "Start external program" field
1. Make sure the "Register for COM interop" checkbox is selected in the "Build" tab of the ADMentor and EAAddInBase project properties
1. Run `registerADMentor.ps1` as _Administrator_ to register the add-in class as an EA extension (adds required entries to the registry)

## Release a new Version

1. Update `version` and `modelVersion` (if necessary) in `AdAddIn.ADTechnology.Technologies`
1. Make sure all model templates are up to date:
  1. Open `ModelTemplates.eap` and recreate the examples and templates if necessary
  1. Export the templates (as XMI) to the according .XML files in `ADTechnology`
  1. Run `realease.ps1` . This script performs mandatory clean up steps on the model templates.
1. Rebuild the _ADMentor.Setup_ project. The build puts the installer into the `ADMentor.Setup\Debug`/`ADMentor.Setup\Release` folder.
1. Make sure all changes have been committed (usual git workflow)
1. Tag the release in your local repository: `git tag vX.Y.Z`
1. Push the tag to remote: `git push --tags`

## Project Layout

### EAAddInBase

EAAddInBase is a small framework for EA Add-Ins that provides core functionalities required by many EA Add-Ins:

* Event Processing
* Context Item Handling
* Menu Handling
* Validation Rules
* C# DSL for defining MDG technologies
* Unified hierarchy for EA entities

### EAAddInBase.Utils

Some useful utility classes and functions that support a more functional and generic programming style in C#. For example implementations of the [Option](http://en.wikipedia.org/wiki/Option_type) and [Unit](http://en.wikipedia.org/wiki/Unit_type) types and various extension methods.

### ADMentor

The ADMentor add-in.

### ADMentor.Setup

The ADMentor installer based on [Visual Studio Installer Projects Extensions](https://visualstudiogallery.msdn.microsoft.com/9abe329c-9bba-44a1-be59-0fbf6151054d).

### ADMentor.ADRepoConnector

A separated add-in that integrates an ADRepo client into ADMentor (experimental).
