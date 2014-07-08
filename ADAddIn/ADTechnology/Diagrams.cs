using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdAddIn.ADTechnology
{
    public static class Diagrams
    {
        public static readonly Diagram ProblemSpace = new Diagram(
            name: "ProblemSpace",
            displayName: "Problem Space",
            description: "Models known issues and their relationship",
            type: DiagramType.Activity,
            toolbox: Toolboxes.ProblemSpaceTools);

        public static readonly Diagram SolutionOverview = new Diagram(
            name: "SolutionOverview",
            displayName: "Solution Overview",
            description: "Models problems occured during a project and how they have been addressed",
            type: DiagramType.Activity,
            toolbox: Toolboxes.SolutionOverviewTools);
    }
}
