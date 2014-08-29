using AdAddIn.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using EAAddInFramework;
using AdAddIn.ADTechnology;

namespace AdAddIn.PopulateDependencies
{
    public class SolutionInstantiationGraph
    {
        private readonly ElementRepository Repo;

        private SolutionInstantiationGraph(ElementRepository repo, DirectedLabeledGraph<ElementInstantiation, EA.Connector> graph)
        {
            Repo = repo;
            Graph = graph;
        }

        public DirectedLabeledGraph<ElementInstantiation, EA.Connector> Graph { get; private set; }

        private static Func<EA.Element, EA.Connector, EA.Element, bool> DependencyGraphFilter =
            DependencyGraph.TraverseOnlyTechnologyConnectors(ADTechnology.Technologies.AD);

        public static Option<SolutionInstantiationGraph> Create(ElementRepository repo, EA.Element solutionItem)
        {
            return from classifier in repo.GetElement(solutionItem.ClassifierID)
                   where classifier.Is(ElementStereotypes.Problem) || classifier.Is(ElementStereotypes.Option)
                   let problemSpace = DependencyGraph.Create(repo, classifier, DependencyGraphFilter)
                   let solution = DependencyGraph.Create(repo, solutionItem, DependencyGraphFilter)
                   select new SolutionInstantiationGraph(repo, Compare(problemSpace, solution));
        }

        private static DirectedLabeledGraph<ElementInstantiation, EA.Connector> Compare(DirectedLabeledGraph<EA.Element, EA.Connector> problemSpace, DirectedLabeledGraph<EA.Element, EA.Connector> solution)
        {
            return problemSpace.MapNodeLabels<ElementInstantiation>(problemItem =>
            {
                var instance = solution.NodeLabels.FirstOption(solutionItem => solutionItem.ClassifierID == problemItem.ElementID);
                return new ElementInstantiation(problemItem, instance);
            });
        }

        public SolutionInstantiationGraph InstantiateSelectedItems(EA.Package package)
        {
            var newGraph = Graph.MapNodeLabels(problemItem =>
            {
                if (problemItem.Selected && !problemItem.Instance.IsDefined)
                {
                    var instance = Repo.Instanciate(problemItem.Element, package);
                    return problemItem.Copy(instance: instance);
                }
                else
                {
                    return problemItem;
                }
            });

            return new SolutionInstantiationGraph(Repo, newGraph);
        }

        /// <summary>
        /// Adds connectors to the solution that exists in the graph but nor yet in the solution.
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="problemSpace"></param>
        /// <returns></returns>
        public Unit InstantiateMissingSolutionConnectors()
        {
            Graph.TraverseEdgesBF((source, edge, target) =>
            {
                source.Instance.Do(solutionSource =>
                {
                    target.Instance.Do(solutionTarget =>
                    {
                        Repo.GetStereotype(edge).Do(stype =>
                        {
                            var connectsAlternativeToProblem =
                                stype == ConnectorStereotypes.HasAlternative && solutionSource.Is(ElementStereotypes.OptionOccurrence);
                            var alreadyExisting = solutionSource.Connectors.Cast<EA.Connector>().Any(c =>
                            {
                                return c.Is(stype) && (c.SupplierID == solutionTarget.ElementID || c.ClientID == solutionTarget.ElementID);
                            });

                            if (!connectsAlternativeToProblem && !alreadyExisting)
                            {
                                stype.Create(solutionSource, solutionTarget);
                            }
                        });
                    });
                });
            });
            return Unit.Instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="solution"></param>
        /// <param name="markedSolutionTree"></param>
        /// <returns></returns>
        public SolutionInstantiationGraph WithSelection(IEnumerable<ElementInstantiation> nodes)
        {
            var markedGraph = Graph.MapNodeLabels(si =>
            {
                var selected = nodes.Any(n => n.Element.ElementGUID == si.Element.ElementGUID && n.Selected);
                return si.Copy(selected: selected);
            });

            return new SolutionInstantiationGraph(Repo, markedGraph);
        }

        /// <summary>
        /// Adds diagram objects for every solution item in the graph to <c>diagram</c>.
        /// </summary>
        /// <param name="diagramRepo"></param>
        /// <param name="diagram"></param>
        /// <param name="problemSpace"></param>
        /// <returns></returns>
        public Unit CreateDiagramElements(DiagramRepository diagramRepo, EA.Diagram diagram)
        {
            var siblings = new Dictionary<ElementInstantiation, int>();

            Graph.TraverseEdgesBF((from, via, to) =>
            {
                var leftHandSiblings = siblings.ContainsKey(from) ? siblings[from] : 0;

                to.Instance.Do(toInstance =>
                {
                    if (!diagramRepo.FindDiagramObject(diagram, toInstance).IsDefined)
                    {
                        from.Instance.Do(fromInstance =>
                        {
                            var parentObject = diagramRepo.FindDiagramObject(diagram, fromInstance).Value;

                            var verticalOffset = leftHandSiblings * 110 - 40;
                            var horizontalOffset = -200 - leftHandSiblings * 20;

                            diagramRepo.AddToDiagram(diagram, toInstance,
                                left: parentObject.left + verticalOffset, right: parentObject.right + verticalOffset,
                                top: parentObject.top + verticalOffset, bottom: parentObject.bottom + verticalOffset);

                            siblings[from] = leftHandSiblings + 1;
                        });
                    }
                });
            });
            diagramRepo.ReloadDiagram(diagram);

            return Unit.Instance;
        }

        /// <summary>
        /// A helper method to create solution instantiation graphs with the correct equality comparers.
        /// </summary>
        /// <param name="edges"></param>
        /// <returns></returns>
        public static SolutionInstantiationGraph Create(
            params Tuple<ElementInstantiation, EA.Connector, ElementInstantiation>[] edges)
        {
            return new SolutionInstantiationGraph(null, DirectedLabeledGraph.Create(
                EqualityComparer<ElementInstantiation>.Default,
                new DependencyGraph.ConnectorComparer(),
                edges));
        }
    }
}
