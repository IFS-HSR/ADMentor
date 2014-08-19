using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdAddIn.ADTechnology
{
    public static class Toolboxes
    {
        public static readonly ToolboxPage ProblemSpaceConncetorsPage = new ToolboxPage(
            name: "ProblemSpaceConnectors",
            displayName: "Problem Space Connectors",
            description: "",
            stereotypes: new []{
                ConnectorStereotypes.HasAlternative,
                ConnectorStereotypes.Includes,
                ConnectorStereotypes.Raises,
                ConnectorStereotypes.Supports,
                ConnectorStereotypes.ConflictsWith,
                ConnectorStereotypes.BoundTo
            });

        public static readonly Toolbox ProblemSpaceTools = new Toolbox(
            name: "ProblemSpaceTools",
            displayName: "Problem Space Tools",
            description: "Elements and connectors for modelling known problems with proposed solutions.",
            pages: new []{
                new ToolboxPage(
                    name: "Elements",
                    displayName: "Elements",
                    description: "",
                    stereotypes: new []{
                        ElementStereotypes.Problem,
                        ElementStereotypes.Option
                    }),
                ProblemSpaceConncetorsPage
            }
        );

        public static readonly Toolbox SolutionOverviewTools = new Toolbox(
            name: "SolutionOverviewTools",
            displayName: "Solution Overview Tools",
            description: "Elements and connectors for modelling the decision process during a project.",
            pages: new []{
                new ToolboxPage(
                    name: "Elements",
                    displayName: "Elements",
                    description: "",
                    stereotypes: new []{
                        ElementStereotypes.ProblemOccurrence,
                        ElementStereotypes.Decision
                    }),
                ProblemSpaceConncetorsPage,
                new ToolboxPage(
                    name: "SolutionOverviewConnectors",
                    displayName: "Solution Overview Connectors",
                    description: "",
                    stereotypes: new []{
                        ConnectorStereotypes.Challenges,
                        ConnectorStereotypes.Overrides
                    })
            }
        );

        public static readonly Toolbox StakeholderTools = new Toolbox(
            name: "StakeholderTools",
            displayName: "Stakeholder Tools",
            description: "",
            pages: new []{
                new ToolboxPage(
                    name: "Elements",
                    displayName: "Elements",
                    description: "",
                    stereotypes: new []{
                        ElementStereotypes.StakeholderRole
                    })
            }
        );
    }
}
