﻿using EAAddInBase.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMentor.ADTechnology
{
    public static class Toolboxes
    {
        public static readonly ToolboxPage ProblemSpaceConncetorsPage = new ToolboxPage(
            name: "ProblemSpaceConnectors",
            displayName: "Problem Space Connectors",
            description: "",
            stereotypes: new[]{
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
                ConnectorStereotypes.AssessesPositively,
                ConnectorStereotypes.AssessesNegatively
            });

        public static readonly Toolbox ProblemSpaceTools = new Toolbox(
            name: "ProblemSpaceTools",
            displayName: "Problem Space Tools",
            description: "Elements and connectors for modelling known problems with proposed solutions.",
            pages: new[]{
                new ToolboxPage(
                    name: "Elements",
                    displayName: "Elements",
                    description: "",
                    stereotypes: new []{
                        ProblemSpace.Problem,
                        ProblemSpace.Option,
                        ProblemSpace.ProblemSpacePackage.Element
                    }),
                ProblemSpaceConncetorsPage,
                QOCConncetorsPage
            }
        );

        public static readonly Toolbox SolutionSpaceTools = new Toolbox(
            name: "SolutionSpaceTools",
            displayName: "Solution Space Tools",
            description: "Elements and connectors for modelling the decision process during a project.",
            pages: new[]{
                new ToolboxPage(
                    name: "Elements",
                    displayName: "Elements",
                    description: "",
                    stereotypes: new []{
                        SolutionSpace.ProblemOccurrence,
                        SolutionSpace.OptionOccurrence,
                        SolutionSpace.SolutionSpacePackage.Element
                    }),
                ProblemSpaceConncetorsPage,
                new ToolboxPage(
                    name: "SolutionSpaceConnectors",
                    displayName: "Solution Space Connectors",
                    description: "",
                    stereotypes: new []{
                        ConnectorStereotypes.Challenges,
                        ConnectorStereotypes.Overrides
                    }),
                new ToolboxPage(
                    name: "SolutionPatterns",
                    displayName: "Solution Space Patterns",
                    description: "",
                    patterns: new []{
                        new UMLPattern(
                            displayName: "Solved Problem",
                            resourceName: "ADMentor.ADTechnology.SolvedProblemPattern.xml")
                    }),
                new ToolboxPage(
                    name: "DecisionCapturingTemplates",
                    displayName: "Decision Capturing Templates",
                    description: "",
                    patterns: new []{
                        new UMLPattern(
                            displayName: "YStatement (split)",
                            resourceName: "ADMentor.ADTechnology.YStatementSplitPattern.xml"),
                        new UMLPattern(
                            displayName: "YStatement (in problem)",
                            resourceName: "ADMentor.ADTechnology.YStatementInProblemPattern.xml"),
                        new UMLPattern(
                            displayName: "YStatement (in option)",
                            resourceName: "ADMentor.ADTechnology.YStatementInOptionPattern.xml"),
                        new UMLPattern(
                            displayName: "ADR (Nygard)",
                            resourceName: "ADMentor.ADTechnology.ADRPattern.xml"),
                        new UMLPattern(
                            displayName: "arc42",
                            resourceName: "ADMentor.ADTechnology.arc42Pattern.xml"),
                        new UMLPattern(
                            displayName: "ISO/IEC/IEEE 42010",
                            resourceName: "ADMentor.ADTechnology.ISO-IEC-IEEE-42010Pattern.xml"),
                        new UMLPattern(
                            displayName: "Tyree Akerman",
                            resourceName: "ADMentor.ADTechnology.TyreeAkermanPattern.xml")
                    })
            }
        );
    }
}
