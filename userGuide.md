# AD Mentor User Guide

## Install AD Mentor

### Software Prerequisites

* Sparx Enterprise Architect (EA) Version 10 or higher

* Microsoft .NET Framework 4.5 (Download at http://www.microsoft.com/en-us/download/details.aspx?id=30653%20)

<!---
Hwardware requirement: about 3MB of disk space (no need to mention)
-->

### Installation

1. Execute "ADSetup.msi".

2. Follow the installation wizard (if you want to uninstall later, use the the standard operating system feature to do so).

## Model a Reusable Problem Space

### Create a new Problem Space

1. Navigate to "Project" -> "New Model..." (or press Ctrl + Shift + m).

2. Select the "Problem Space" model from the "AD Mentor" technology.

    ![Create a new Problem Space](docs/newProblemSpace.png?raw=true)

##	Add Problems/Options to the Problem Space

#### Using the Toolbox

1. Open a "Problem Space" Diagram.

2. Drag and drop the desired element type to the diagram.

    ![](docs/toolbox.png?raw=true)
 
#### Using Quick Links

1. Select an existing element in a Problem Space diagram, e.g. a Problem or an Option.

2. Click on the Quick Linker arrow at the top right corner of the shape and drag it to the position where the new element should be created.

    ![](docs/quickLink.png?raw=true)

3. Select the connection type that should be used to associate the new element with the selected element, as well as the type of the new element (e.g. Option or Problem).

    ![](docs/quickLink2.png?raw=true)

    The above quick link reads as "Problem 1 should be addressed by a new Option".

    Quick links can also be used to connect two existing elements.

## Create a Tailored Problem Space Export

A Problem Space may contain information that should not be used to create a concrete solution. This information may include confidential data or information that is clearly not related to the solution. This is why AD Mentor offers a feature to export a subset of a Problem Space that is specifically tailored to the needs of a particular solution project.

1. Right click on a package containing Problem Space items in the Project Browser.

2. Select the "Extensions" -> "AD Mentor" -> "Tailor Problem Space" menu item. AD Mentor may now take up to a minute, or more for very large models, to calculate the required information.
 
    ![](docs/tailor.png?raw=true)

3. Adapt the filter that you want to apply on the Problem Space in the upper half of the "Tailor Package Export" dialog. The lower half of the dialog lists the entities that match the selected filter and will be exported when clicking "Export". You can also select/deselect individual entities to specify an even finer grained tailoring of the problem space.

    ![](docs/tailor2.png?raw=true)

4. When you click on "export", AD Mentor asks for a location for the XML export. Choose an appropriate path and filename and start the export by clicking "Save". Depending on the size of the Problem Space, the export may take several seconds.

The entity filters allow selecting elements with desired properties. These properties include the tagged values defined by ADMentor, keywords and other element properties defined by Enterprise Architect and the type of the element.

The top-level nodes of the filter tree form a conjunction (logical and). Hence, elements must match every selected top-level filter to be included in the export. The options in each top-level filter form a disjunction (logical or). An element must match one of the selected properties.

For instance, the following filter selection accepts elements that have an "Organizational Reach" of "Division" and are in the "Project Stage" "Elaboration" or "Inception":

![](docs/tailor3.png?raw=true)
 
Diagrams and Packages are included in the export if they match the selected filter or they contain elements that match a selected filter.
And finally, there is an additional rule, that an Option derives properties from a Problem when these two elements are connected by an "isAddressedBy" relationship and the property is not present in the Option. For example, an Option does not have a "Project Stage" tag, but derives its project stage tag from all Problems that are addressed by this Option.

##	Import a Tailored Problem Space into another Project

1. Open an EA Project file.

2. Right click on the Root Model in the Project Browser and select the "Import Model from XMI..." menu item (or press Ctrl + Alt + I).

    ![](docs/import.png?raw=true)

3. Enter the location of the tailored XML export file and click on "Import" to start the import.
   
    If you import the XMI into the same project file as the one used to generate the export, you have to select the "Strip GUIDs" option to prevent collisions.

## Create a Concrete Solution Space

### Manually Populate a Solution

1. Create a new Solution Space (in the same way as you create a problem space, see Section 1.3 for instructions).

2. Open the Solution Space diagram that is contained in the new Solution Space. You can also create Solution Space diagrams manually.

3. Drag and drop items from the Problem Space into the Solution Space diagram.

    ![](docs/populate.png?raw=true)

4. Enterprise Architect should now open the "Paste Element" dialog. Select "Instance (Object)" or "Instance (ADMentor::adProblemOccurrence)" in the "Paste as" drop down menu (it doesn not matter which of these two menu entries you choose). The "Copy Connectors" checkbox has no implications on the outcome, and you can leave it as is. Confirm the element instantiation by clicking "OK".
 
You can also automatically add instances of related Problems/Options to existing Solution Space items: Right click on an Option/Problem Occurrence in a Solution Space diagram and choose the "Extensions" -> "AD Mentor" -> "Establish Dependencies from Problem Space" menu item.

This will open the "Populate Dependencies Wizard". You can use this wizard to create instances of related elements from the Problem Space in the Solution. When you select related elements and press "Create", AD Mentor automatically instantiates them accordingly and adds the new instances to the currently open diagram.
 
### Automatically Instantiate a Complete Problem Space into a new Solution

1. Right click on a package that contains Problem Space items in the Project Browser.

2. Select the "Extensions" -> "AD Mentor" -> "Create Solution Space from Problem Space" menu item.

    ![](docs/populate2.png?raw=true)

3. Enter a name for the solution to be created and click "Create".

4. AD Mentor now creates a new Solution Space that contains instances of all Problems and Options from the selected package including the according connectors and diagrams. AD Mentor tries to reproduce instantiated diagrams as accurate as possible and also includes references to other elements like notes or requirements that were not instantiated. Unfortunately not all diagram objects can easily be copied. For example embedded images are not supported.

## Document the Decision Making Process

### Update the Decision State of an Option Occurrence

1. Select an Option Occurrence in a Solution Space diagram.

2. Open the "Tagged Values" tab that appears near the properties editor (in the default layout of the EA user interface, which can be customized by the user). If you can't find the tab, you can open it via the main menu under "View" -> "Tagged Values" or by pressing Ctrl + Shift + 6.

    ![](docs/taggedValues.png?raw=true)

3. Change the "State" value accordingly to your decision. AD Mentor will automatically update the state of associated problems.

AD Mentor also offers a shortcut for documenting past decisions. Just select an Option Occurrence and open in the context menu "Extensions" -> "AD Mentor" -> "Choose Selected and Neglect not chosen Alternatives". This will set the state of the selected Option Occurrence to "Chosen" and the state of all other Options of the associated Problem Occurrence that are not "Chosen" to "Neglected".

You can also neglect all options of a problem and set the problem to state "not applicable" with a problem-level shortcut: "Extensions" -> "AD Mentor" -> "Neglect all Alternatives".

### Find not yet Decided Problem Occurrences

1. Right click on your Solution Space package in the project browser and select the "Package Browser" -> "Standard View" menu item.

2. Right click an item with stereotype "adProblemOccurrence" and choose the menu item "Add Tag Value Column".

    ![](docs/packageBrowser.png?raw=true)

3. Select the radio button "Tagged Values from the selected Element" in the "Add Tag Value Column" dialog.

4. Select the "Problem State" entry in the "Tagged Value" drop down and click "OK".

5. The package browser now contains a column "Problem State" that displays the decision state of Problem Occurrences. By dragging the "Problem State" column header to the area labeled "Drag a column header here to group by that column" you can group the package contents by the decision state. Additionally, you can specify filters on the decision state by clicking the "Toggle Filter Bar" button .

Further customization of the Standard View of the Package Browser is possible (this is standard EA functionality). For instance, you can add some of the Tagged Values defined in Section 10 to turn the Package Browser into a full decision backlog. To save and load such customizations, go to the "Columns Layout" entry in the context menu of the Package Browser. 

## Query Metrics on Problem and Solution Spaces

1. Right click on a package in the project browser and select the "AD Mentor" -> "Package Metrics".

2. AD Mentor will now calculate several metrics for you (this may take several seconds).

    ![](docs/metrics.png?raw=true) 

## Validate Problem and Solution Spaces

AD Mentor provides several validation rules for Enterprise Architects built in validation facility. Currently, the following checks are implemented:

* Warning: Problems without associated Options (via AdressedBy links)

* Warning: Options that are not associated with a Problem (via AdressedBy links)

* Warning: Options that address more than one Problem

* Warning: Problem Occurrences without associated Option Occurrences

* Warning: Problem Occurrences that are not associated to a Problem Occurrence

* Error: Problem Occurrences with a state that does not map to the states of the associated Options (Inconsistent State)

* Error: Problem Occurrences with two or more chosen Options that are connected with a "conflictsWith" connector (conflicting Options chosen)

* Warning: ADMentor entities with an outdated model version

* Warning: Elements that are not part of any diagram

Perform the following steps to run EA's validation:

1. Open "Project" -> "Model Validation" -> "Configure..." in the main menu.

2. Enable the "AD Mentor" validation rules and confirm with "OK" (you may want to, but do not have to, deselect the other options). 

    ![](docs/validation.png?raw=true)

3. Select the package that you want to validate in the project browser.

4. Start the validation via "Project" -> "Model Validation" -> "Validate Selected" (or Ctrl + Alt + V)

5. The validation writes all found issues to the "System Output" tab. This may take several seconds or even minutes, depending on the size of the selected package.

## Generate AD Reports

This repository offers two pre-defined document templates to generate AD reports.

1. Download the [XML reference file](docs/adReports.xml).
1. Open "Project" -> "Model Import/Export" -> "Import Reference Data..." in EA.
1. Select the downloaded XML reference file, select the "RTF Document Templates" dataset and press "Import".
1. Press <kbd>F8</kbd> to create reports with one of the imported templates ("ADReportBrief" or "ADReportFull").

## FAQ

* __Which versions of Enterprise Architect are supported by the AD Mentor Add-In?__

    The current release is targeted for Enterprise Architect (EA) Version 10. EA Version 11 is supported, but behavior may slightly differ from the description in this walkthrough.
    
* __What are the usage scenarios for ADMentor?__

    ADMentor can be used for guidance modeling as an explicit approach to Architectural Knowledge Management (AKM) and for Architectural Decision (AD) capturing after the fact. AD capturing can, but does not have to be supported by a Problem Space (i.e., a set of problem space packages collecting recurring problems and options), but does not have to be. New problems and options can also be discovered while capturing recently made decisions, e.g. on exploratory first-of-a-kind projects. AKM guidance models are living models that can help architects organizing their decision making, but do not aim to be complete or comprehensive. They cannot guarantee that the made decisions are adequate, as project-specific context and requirements have to drive the problem prioritization and option selection. They should be viewed as compasses or checklists.     

* __Can I use AD Mentor and Decision Architect for EA (https://decisions.codeplex.com/) in the same project?__

    Yes, AD Mentor is designed to cause as few inferences with other add-ins as possible. However, there is (currently) no semantic integration of Decision Architect entities into AD Mentor. That said, it is possible to use general-purpose relationship links such as "trace" to connect Decision Architect decision elements (e.g.in the relationship viewpoint) to problem and/or option occurrences in ADMentor solution spaces. The Relationship viewpoint in Decision Architect is recommended for such integrated, hybrid use of the two architectural decision modeling add-ins.
    
* __Where can I find more information about the syntax and the semantics of the relationship links between problems and options (in problem space packages)?__

    The notation for relationships such as "adAddressedBy" and "adSuggests" is inspired by UML and other diagram/model element types in EA. Their semantics are explained in this IEEE/IFIP WICSA 2015 article (also see referenced related work): http://www.ifs.hsr.ch/fileadmin/user_upload/customers/ifs.hsr.ch/Home/projekte/ADMentor-WICSA2015ubmissionv11nc.pdf 

* __Why does the "Create Solution Space from Problem Space" menu command omit instantiation of some connectors in my Problem Space?__

    EA prohibits the automated creation of some connectors that can be created manually in the user interface. E.g. add-ins cannot create Association connectors with a Package as a target.

* __How can I find problem occurrences that have been created from the problems in my problem space?__

    Right click on the problem in question and select "Find in all Diagrams" (Ctrl + U).
    
* __Can I capture Architectural Decisions at the code level as well (rather than in a modeling tool)?__

    Have a look at this project, which suggests to use AOP for this purpose: https://github.com/koppor/embedded-adl 

## Known Limitations and Issues (ADMentor Version 1.2)

* The commands "Establish Dependencies from Problem Space" and "Create Solution Space from Problem Space" save all currently open diagrams before execution.

* Adding Option Occurrences that addresses a Problem Occurrence vie "Establish Dependencies from Problem Space" does currently not update the state of the Problem Occurrence.

* "Establish Dependencies from Problem Space" and "Project" -> "Model Validation" may terminate EA under some non-reproducible circumstances. Until now, data loss in the affected project file due to this bug did not occur.

* The XML files generated by "Tailor Problem Space" may contain more information than visible in EA after importing the file. This could lead to leaked confidential information in some rare cases.Importing a tailored XMI into the same project that was used to export the file can lead to unexpected results - imported diagrams contain all original elements, even the ones that have been filtered out.
