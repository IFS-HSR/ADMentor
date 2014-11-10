﻿using EAAddInFramework.MDGBuilder;
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
                ConnectorStereotypes.AddressedBy,
                ConnectorStereotypes.Raises,
                ConnectorStereotypes.Suggests,
                ConnectorStereotypes.ConflictsWith,
                ConnectorStereotypes.BoundTo
            });

        public static readonly ToolboxPage QOCConncetorsPage = new ToolboxPage(
            name: "QOCConnectors",
            displayName: "QOC Connectors",
            description: "",
            stereotypes: new[]{
                ConnectorStereotypes.PositiveAssessement,
                ConnectorStereotypes.NegativeAssessement
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
                        ProblemSpace.Problem,
                        ProblemSpace.Option,
                        ProblemSpace.ProblemSpacePackage
                    }),
                ProblemSpaceConncetorsPage,
                QOCConncetorsPage
            }
        );

        public static readonly Toolbox SolutionSpaceTools = new Toolbox(
            name: "SolutionSpaceTools",
            displayName: "Solution Space Tools",
            description: "Elements and connectors for modelling the decision process during a project.",
            pages: new []{
                new ToolboxPage(
                    name: "Elements",
                    displayName: "Elements",
                    description: "",
                    stereotypes: new []{
                        SolutionSpace.ProblemOccurrence,
                        SolutionSpace.OptionOccurrence,
                        SolutionSpace.SolutionSpacePackage
                    }),
                ProblemSpaceConncetorsPage,
                new ToolboxPage(
                    name: "SolutionSpaceConnectors",
                    displayName: "Solution Space Connectors",
                    description: "",
                    stereotypes: new []{
                        ConnectorStereotypes.Challenges,
                        ConnectorStereotypes.Overrides
                    })
            }
        );
    }
}
