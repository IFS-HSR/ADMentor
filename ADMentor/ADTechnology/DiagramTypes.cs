using EAAddInBase.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMentor.ADTechnology
{
    public static class DiagramTypes
    {
        public static readonly Diagram ProblemSpace = new Diagram(
            name: "ProblemSpace",
            displayName: "Problem Space",
            description: "ADMentor Problem Space Diagrams visualize recurring architectural design problems and known solutions to them. " +
                         "They also visualize the relationships between these model elements.",
            type: DiagramType.Activity,
            toolbox: Toolboxes.ProblemSpaceTools);

        public static readonly Diagram QOC = new Diagram(
            name: "QOC",
            displayName: "Questions, Options and Criteria",
            description: "Question, Options and Criteria (QOC) Diagrams visualize recurring architectural design problems similar to Problem Space Diagrams. " + 
                         "Additionally, they support detailed visualization based on optional criteria.",
            type: DiagramType.Activity,
            toolbox: Toolboxes.ProblemSpaceTools);

        public static readonly Diagram SolutionOverview = new Diagram(
            name: "SolutionOverview",
            displayName: "Solution Overview",
            description: "ADMentor Solution Space Diagrams collect architectural design problems that occured on a particular project and " +
                         "the design options investigated and chosen to address them.",
            type: DiagramType.Activity,
            toolbox: Toolboxes.SolutionSpaceTools);
    }
}
