using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
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
        public static Option<LabeledTree<SolutionInstantiation, EA.Connector>> Create(ElementRepository repo, EA.Element solutionItem)
        {
            return from classifier in repo.GetElement(solutionItem.ClassifierID)
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
                return LabeledTree.Node(new SolutionInstantiation(problemSpace.Label, s.Label.AsOption()), edges);
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
                   let sEdge = solution.FirstOption(edge => edge.Label.Stereotype == psEdge.Label.Stereotype && edge.Target.Label.ClassifierID == psEdge.Target.Label.ElementID)
                   select LabeledTree.Edge(psEdge.Label, Compare(psEdge.Target, sEdge.Select(e => e.Target)));
        }

        public static LabeledTree<SolutionInstantiation, EA.Connector> InstantiateSelectedItems(ElementRepository repo, EA.Package package, LabeledTree<SolutionInstantiation, EA.Connector> problemSpace)
        {
            return problemSpace.TransformTopDown((parent, connector, child) =>
            {
                if (!child.Instance.IsDefined && child.Selected)
                {
                    var sourceElement = parent.Instance.Value;
                    var targetElement = repo.Instanciate(child.Element, package).Value;

                    var connectorSType = repo.GetStereotype(connector).Value;
                    connectorSType.Create(sourceElement, targetElement);

                    return child.Copy(instance: targetElement.AsOption());
                }
                else
                {
                    return child;
                }
            });
        }

        public static Unit CreateDiagramElements(ElementRepository repo, DiagramRepository diagramRepo, EA.Diagram diagram, LabeledTree<SolutionInstantiation, EA.Connector> problemSpace)
        {
            var siblings = new Dictionary<SolutionInstantiation, int>();

            problemSpace.TraverseTopDown((parent, connector, child) =>
            {
                var leftHandSiblings = siblings.ContainsKey(parent) ? siblings[parent] : 0;

                child.Instance.Do(instance =>
                {
                    if (!diagramRepo.FindDiagramObject(diagram, instance).IsDefined)
                    {
                        var parentObject = diagramRepo.FindDiagramObject(diagram, parent.Instance.Value).Value;

                        var verticalOffset = leftHandSiblings * 110 - 40;
                        var horizontalOffset = -200 - leftHandSiblings * 20;

                        diagramRepo.AddToDiagram(diagram, instance,
                            parentObject.left + verticalOffset, parentObject.top + horizontalOffset,
                            parentObject.right - parentObject.left, parentObject.bottom - parentObject.top);

                        siblings[parent] = leftHandSiblings + 1;
                    }
                });
            });
            diagramRepo.ReloadDiagram(diagram);

            return Unit.Instance;
        }
    }
}
