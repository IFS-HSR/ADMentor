using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdAddIn.ADTechnology
{
    public static class Technologies
    {
        public static readonly MDGTechnology AD = new MDGTechnology(
            id: "ADMentor",
            name: "AD Mentor",
            version: "1.0.0",
            modelVersion: 2,
            description: "Modelling Language for documentation and reuse of architectural decisions",
            diagrams: new Diagram[]{
                        ADTechnology.DiagramTypes.ProblemSpace,
                        ADTechnology.DiagramTypes.SolutionOverview
                    },
            modelTemplates: new ModelTemplate[]{
                        new ModelTemplate(
                            name: "Problem Space",
                            description: "",
                            icon: ModelIcon.ClassModel,
                            resourceName: "AdAddIn.ADTechnology.ProblemSpaceTemplate.xml"),
                        new ModelTemplate(
                            name: "Solution Space",
                            description: "",
                            icon: ModelIcon.DeploymentModel,
                            resourceName: "AdAddIn.ADTechnology.SolutionSpaceTemplate.xml"),
                        new ModelTemplate(
                            name: "AD Mentor Demo",
                            description: "",
                            icon: ModelIcon.SimpleModel,
                            resourceName: "AdAddIn.ADTechnology.DemoTemplate.xml")
                    });
    }
}
