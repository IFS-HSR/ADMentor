using EAAddInBase.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMentor.ADTechnology
{
    public static class Technologies
    {
        public static readonly MDGTechnology AD = new MDGTechnology(
            id: "ADMentor",
            name: "AD Mentor",
            version: "1.1.1",
            modelVersion: 3,
            description: "Modelling Language for documentation and reuse of architectural decisions",
            diagrams: new Diagram[]{
                        DiagramTypes.ProblemSpace,
                        DiagramTypes.QOC,
                        DiagramTypes.SolutionOverview
                    },
            modelTemplates: new ModelTemplate[]{
                        new ModelTemplate(
                            name: "Problem Space",
                            description: "",
                            icon: ModelIcon.ClassModel,
                            resourceName: "ADMentor.ADTechnology.ProblemSpaceTemplate.xml"),
                        new ModelTemplate(
                            name: "Solution Space",
                            description: "",
                            icon: ModelIcon.DeploymentModel,
                            resourceName: "ADMentor.ADTechnology.SolutionSpaceTemplate.xml"),
                        new ModelTemplate(
                            name: "AD Mentor Demo",
                            description: "",
                            icon: ModelIcon.SimpleModel,
                            resourceName: "ADMentor.ADTechnology.DemoTemplate.xml")
                    });
    }
}
