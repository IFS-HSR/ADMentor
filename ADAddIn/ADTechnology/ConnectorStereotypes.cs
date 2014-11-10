using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdAddIn.ADTechnology
{
    public static class ConnectorStereotypes
    {
        public static readonly ConnectorStereotype AddressedBy = new ConnectorStereotype(
            name: "adAddressedBy",
            displayName: "Addressed By",
            reverseDisplayName: "Addresses",
            type: ConnectorType.Association,
            direction: Direction.Unspecified,
            compositionKind: CompositionKind.AggregateAtSource,
            connects: new[]{
                new Connection(from: ProblemSpace.Problem, to: ProblemSpace.Option),
                new Connection(from: SolutionSpace.ProblemOccurrence, to: SolutionSpace.OptionOccurrence)
            });

        public static readonly ConnectorStereotype Raises = new ConnectorStereotype(
            name: "adRaises",
            displayName: "Raises",
            reverseDisplayName: "Raised By",
            type: ConnectorType.Association,
            direction: Direction.SourceToDestination,
            connects: new[]{
                new Connection(from: ProblemSpace.Option, to: ProblemSpace.Problem),
                new Connection(from: SolutionSpace.OptionOccurrence, to: SolutionSpace.ProblemOccurrence),
                new Connection(from: ProblemSpace.Problem, to: ProblemSpace.Problem),
                new Connection(from: SolutionSpace.ProblemOccurrence, to: SolutionSpace.ProblemOccurrence),
                new Connection(from: ProblemSpace.Option, to: ProblemSpace.ProblemSpacePackage),
                new Connection(from: SolutionSpace.OptionOccurrence, to: SolutionSpace.SolutionSpacePackage),
                new Connection(from: ProblemSpace.Problem, to: ProblemSpace.ProblemSpacePackage),
                new Connection(from: SolutionSpace.ProblemOccurrence, to: SolutionSpace.SolutionSpacePackage),
                new Connection(from: ProblemSpace.ProblemSpacePackage, to: ProblemSpace.ProblemSpacePackage),
                new Connection(from: SolutionSpace.SolutionSpacePackage, to: SolutionSpace.SolutionSpacePackage)
            });

        public static readonly ConnectorStereotype Suggests = new ConnectorStereotype(
            name: "adSuggests",
            displayName: "Suggests",
            reverseDisplayName: "Suggested By",
            type: ConnectorType.Dependency,
            direction: Direction.SourceToDestination,
            connects: new[]{
                new Connection(from: ProblemSpace.Option, to: ProblemSpace.Option),
                new Connection(from: SolutionSpace.OptionOccurrence, to: SolutionSpace.OptionOccurrence),
                new Connection(from: ElementType.Issue.DefaultStereotype, to: SolutionSpace.OptionOccurrence),
                new Connection(from: ElementType.Requirement.DefaultStereotype, to: SolutionSpace.OptionOccurrence)
            });

        public static readonly ConnectorStereotype ConflictsWith = new ConnectorStereotype(
            name: "adConflictsWith",
            displayName: "Conflicts With",
            type: ConnectorType.Dependency,
            direction: Direction.BiDirectional,
            connects: new[]{
                new Connection(from: ProblemSpace.Option, to: ProblemSpace.Option),
                new Connection(from: SolutionSpace.OptionOccurrence, to: SolutionSpace.OptionOccurrence)
            });

        public static readonly ConnectorStereotype BoundTo = new ConnectorStereotype(
            name: "adBoundTo",
            displayName: "Bound To",
            type: ConnectorType.Dependency,
            direction: Direction.BiDirectional,
            connects: new[]{
                new Connection(from: ProblemSpace.Option, to: ProblemSpace.Option),
                new Connection(from: SolutionSpace.OptionOccurrence, to: SolutionSpace.OptionOccurrence)
            });

        public static readonly ConnectorStereotype Challenges = new ConnectorStereotype(
            name: "adChallenges",
            displayName: "Challenges",
            reverseDisplayName: "Challenged By",
            type: ConnectorType.Dependency,
            direction: Direction.SourceToDestination,
            connects: new[]{
                new Connection(from: ElementType.Issue.DefaultStereotype, to: SolutionSpace.OptionOccurrence),
                new Connection(from: ElementType.Requirement.DefaultStereotype, to: SolutionSpace.OptionOccurrence)
            });

        public static readonly ConnectorStereotype Overrides = new ConnectorStereotype(
            name: "adOverrides",
            displayName: "Overrides",
            reverseDisplayName: "Overriden By",
            type: ConnectorType.Association,
            direction: Direction.SourceToDestination,
            connects: new[]{
                new Connection(from: SolutionSpace.OptionOccurrence, to: SolutionSpace.OptionOccurrence)
            });

        public static readonly ConnectorStereotype PositiveAssessement = new ConnectorStereotype(
            name: "adPositiveAssessement",
            displayName: "Positive Assessement",
            reverseDisplayName: "Positive Assessed By",
            type: ConnectorType.Association,
            direction: Direction.Unspecified,
            connects: new[]{
                new Connection(from: ElementType.Requirement.DefaultStereotype, to: ProblemSpace.Option)
            });

        public static readonly ConnectorStereotype NegativeAssessement = new ConnectorStereotype(
            name: "adNegativeAssessement",
            displayName: "Negative Assessement",
            reverseDisplayName: "Negative Assessed By",
            type: ConnectorType.Dependency,
            direction: Direction.Unspecified,
            connects: new[]{
                new Connection(from: ElementType.Requirement.DefaultStereotype, to: ProblemSpace.Option)
            });
    }
}
