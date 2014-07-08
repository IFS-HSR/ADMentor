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
            description: "Modelling Language for documentation and reuse of architectural decisions",
            diagrams: new Diagram[]{
                        ADTechnology.Diagrams.ProblemSpace,
                        ADTechnology.Diagrams.SolutionOverview
                    },
            modelTemplates: new ModelTemplate[]{
                        new ModelTemplate(
                            name: "Problem Space",
                            description: "",
                            icon: ModelIcon.ComponentModel,
                            resourceName: "AdAddIn.ADTechnology.ProblemSpaceTemplate.xml"),
                        new ModelTemplate(
                            name: "Solution Overview",
                            description: "",
                            icon: ModelIcon.DynamicModel,
                            resourceName: "AdAddIn.ADTechnology.SolutionOverviewTemplate.xml"),
                        new ModelTemplate(
                            name: "AD Mentor Demo",
                            description: "",
                            icon: ModelIcon.SimpleModel,
                            resourceName: "AdAddIn.ADTechnology.DemoTemplate.xml")
                    });
    }
}
