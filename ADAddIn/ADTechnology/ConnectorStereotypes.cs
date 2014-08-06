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
        /// <summary>
        /// Problem -Has Alternative-> Option
        /// </summary>
        public static readonly ConnectorStereotype HasAlternative = new ConnectorStereotype(
            name: "adHasAlternative",
            displayName: "Has Alternative",
            reverseDisplayName: "Alternative For",
            type: ConnectorType.Association,
            direction: Direction.Unspecified,
            compositionKind: CompositionKind.AggregateAtSource,
            connects: new[]{
                new Connection(from: ElementStereotypes.Problem, to: ElementStereotypes.Option),
                new Connection(from: ElementStereotypes.ProblemOccurrence, to: ElementStereotypes.Decision)
            });

        /// <summary>
        /// Option -Raises-> Problem
        /// </summary>
        public static readonly ConnectorStereotype Raises = new ConnectorStereotype(
            name: "adRaises",
            displayName: "Raises",
            reverseDisplayName: "Raised By",
            type: ConnectorType.Association,
            direction: Direction.SourceToDestination,
            connects: new[]{
                new Connection(from: ElementStereotypes.Option, to: ElementStereotypes.Problem),
                new Connection(from: ElementStereotypes.Decision, to: ElementStereotypes.ProblemOccurrence)
            });

        /// <summary>
        /// Problem -Includes-> Problem
        /// </summary>
        public static readonly ConnectorStereotype Includes = new ConnectorStereotype(
            name: "adIncludes",
            displayName: "Includes",
            reverseDisplayName: "Included In",
            type: ConnectorType.Association,
            direction: Direction.SourceToDestination,
            connects: new[]{
                new Connection(from: ElementStereotypes.Problem, to: ElementStereotypes.Problem),
                new Connection(from: ElementStereotypes.ProblemOccurrence, to: ElementStereotypes.ProblemOccurrence)
            });

        /// <summary>
        /// Option -Supports-> Option
        /// </summary>
        public static readonly ConnectorStereotype Supports = new ConnectorStereotype(
            name: "adSupports",
            displayName: "Supports",
            reverseDisplayName: "Supported By",
            type: ConnectorType.Dependency,
            direction: Direction.SourceToDestination,
            connects: new[]{
                new Connection(from: ElementStereotypes.Option, to: ElementStereotypes.Option),
                new Connection(from: ElementStereotypes.Decision, to: ElementStereotypes.Decision)
            });

        /// <summary>
        /// Option <-ConflictsWith-> Option
        /// </summary>
        public static readonly ConnectorStereotype ConflictsWith = new ConnectorStereotype(
            name: "adConflictsWith",
            displayName: "Conflicts With",
            type: ConnectorType.Dependency,
            direction: Direction.BiDirectional,
            connects: new[]{
                new Connection(from: ElementStereotypes.Option, to: ElementStereotypes.Option),
                new Connection(from: ElementStereotypes.Decision, to: ElementStereotypes.Decision)
            });

        /// <summary>
        /// Option <-IsBoundTo-> Option
        /// </summary>
        public static readonly ConnectorStereotype BoundTo = new ConnectorStereotype(
            name: "adBoundTo",
            displayName: "Bound To",
            type: ConnectorType.Dependency,
            direction: Direction.BiDirectional,
            connects: new[]{
                new Connection(from: ElementStereotypes.Option, to: ElementStereotypes.Option),
                new Connection(from: ElementStereotypes.Decision, to: ElementStereotypes.Decision)
            });

        /// <summary>
        /// Issue/Requirement -Challenges-> Decision
        /// </summary>
        public static readonly ConnectorStereotype Challenges = new ConnectorStereotype(
            name: "adChallenges",
            displayName: "Challenges",
            reverseDisplayName: "Challenged By",
            type: ConnectorType.Dependency,
            direction: Direction.SourceToDestination,
            connects: new[]{
                new Connection(from: ElementType.Issue.DefaultStereotype, to: ElementStereotypes.Decision),
                new Connection(from: ElementType.Requirement.DefaultStereotype, to: ElementStereotypes.Decision)
            });

        /// <summary>
        /// Decision -Overrides-> Decision
        /// </summary>
        public static readonly ConnectorStereotype Overrides = new ConnectorStereotype(
            name: "adOverrides",
            displayName: "Overrides",
            reverseDisplayName: "Overriden By",
            type: ConnectorType.Association,
            direction: Direction.SourceToDestination,
            connects: new[]{
                new Connection(from: ElementStereotypes.Decision, to: ElementStereotypes.Decision)
            });
    }
}
