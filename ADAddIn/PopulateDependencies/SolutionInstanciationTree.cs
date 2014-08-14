using AdAddIn.ADTechnology;
using EAAddInFramework;
using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.PopulateDependencies
{
    public static class SolutionInstantiationTree
    {
        /// <summary>
        /// Creates a tree of possible items from the problem space that may be added to the solution.
        /// 
        /// The root of the tree corresponds with the problem space item that has been used to instantiate <c>solutionItem</c>.
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="solutionItem">The problem space item that has been used to instantiate this element becomes the root of the tree</param>
        /// <returns><c>None</c> if <c>solutionItem</c> has no valid classifier. Otherwise a tree representing the problem space.</returns>
        public static Option<LabeledTree<SolutionInstantiation, EA.Connector>> Create(EA.Repository repo, EA.Element solutionItem)
        {
            return from classifier in repo.TryGetElement(solutionItem.ClassifierID)
                   where classifier.Is(ElementStereotypes.Problem)
                   let problemSpace = DependencyTree.Create(repo, classifier)
                   let solution = DependencyTree.Create(repo, solutionItem)
                   select Compare(problemSpace, solution.AsOption());
        }

        private static LabeledTree<SolutionInstantiation, EA.Connector> Compare(LabeledTree<EA.Element, EA.Connector> problemSpace, Option<LabeledTree<EA.Element, EA.Connector>> solution)
        {
            return solution.Match(s =>
            {
                var edges = Compare(problemSpace.Edges, s.Edges);
                return LabeledTree.Node(new SolutionInstantiation(problemSpace.Label, s.Label), edges);
            }, () =>
            {
                var edges = from edge in problemSpace.Edges
                            select LabeledTree.Edge(edge.Label, Compare(edge.Target, Options.None<LabeledTree<EA.Element, EA.Connector>>()));
                return LabeledTree.Node(new SolutionInstantiation(problemSpace.Label), edges);
            });
        }

        private static IEnumerable<LabeledTree<SolutionInstantiation, EA.Connector>.Edge> Compare(IEnumerable<LabeledTree<EA.Element, EA.Connector>.Edge> problemSpace, IEnumerable<LabeledTree<EA.Element, EA.Connector>.Edge> solution)
        {
            return from psEdge in problemSpace
                   let sEdge = solution.FirstOption(edge => edge.Label.Stereotype == psEdge.Label.Stereotype && problemSpace.Any(ps => ps.Target.Label.ElementID == edge.Target.Label.ClassifierID))
                   select LabeledTree.Edge(psEdge.Label, Compare(psEdge.Target, sEdge.Select(e => e.Target)));
        }

        public static LabeledTree<SolutionInstantiation, EA.Connector> InstantiateSelectedItems(EA.Repository repo, EA.Package package, LabeledTree<SolutionInstantiation, EA.Connector> problemSpace)
        {
            var before = problemSpace;
            var after = problemSpace.Select((parent, connector, child) =>
            {
                if (!child.Instance.IsDefined && child.Selected)
                {
                    var sourceElement = parent.Instance.Value;
                    var targetElement = child.Element.Instanciate(package).Value;

                    var connectorSType = connector.GetStereotype().Value;
                    connectorSType.Create(sourceElement, targetElement);

                    return child.Copy(instance: targetElement);
                }
                else
                {
                    return child;
                }
            });
            return after;
        }
    }

    public class SolutionInstantiation : IEquatable<SolutionInstantiation>
    {
        public SolutionInstantiation(EA.Element element, EA.Element instance = null, bool selected = false)
        {
            Element = element;
            Instance = instance.AsOption();
            Selected = selected;
        }

        public EA.Element Element { get; private set; }

        public Option<EA.Element> Instance { get; private set; }

        public bool Selected { get; private set; }

        public bool Equals(SolutionInstantiation other)
        {
            return Element.ElementGUID == other.Element.ElementGUID
                && Instance.IsDefined == other.Instance.IsDefined
                && Instance.Match(e => e.ElementGUID == other.Instance.Value.ElementGUID, () => true)
                && Selected == other.Selected;
        }

        public SolutionInstantiation Copy(EA.Element element = null, EA.Element instance = null, bool? selected = null)
        {
            return new SolutionInstantiation(element ?? Element, instance ?? Instance.GetOrDefault(), selected ?? Selected);
        }
    }
}
