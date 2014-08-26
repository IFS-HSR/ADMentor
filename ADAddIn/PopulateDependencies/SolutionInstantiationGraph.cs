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

        private SolutionInstantiationGraph(ElementRepository repo, DirectedLabeledGraph<SolutionInstantiation, EA.Connector> graph)
        {
            Repo = repo;
            Graph = graph;
        }

        public DirectedLabeledGraph<SolutionInstantiation, EA.Connector> Graph { get; private set; }

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

        private static DirectedLabeledGraph<SolutionInstantiation, EA.Connector> Compare(DirectedLabeledGraph<EA.Element, EA.Connector> problemSpace, DirectedLabeledGraph<EA.Element, EA.Connector> solution)
        {
            return problemSpace.MapNodeLabels<SolutionInstantiation>(problemItem =>
            {
                var instance = solution.NodeLabels.FirstOption(solutionItem => solutionItem.ClassifierID == problemItem.ElementID);
                return new SolutionInstantiation(problemItem, instance);
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
        public SolutionInstantiationGraph WithSelection(IEnumerable<SolutionInstantiation> nodes)
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
            var siblings = new Dictionary<SolutionInstantiation, int>();

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
                                parentObject.left + verticalOffset, parentObject.top + horizontalOffset,
                                parentObject.right - parentObject.left, parentObject.bottom - parentObject.top);

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
            params Tuple<SolutionInstantiation, EA.Connector, SolutionInstantiation>[] edges)
        {
            return new SolutionInstantiationGraph(null, DirectedLabeledGraph.Create(
                EqualityComparer<SolutionInstantiation>.Default,
                new DependencyGraph.ConnectorComparer(),
                edges));
        }
    }

    public class SolutionInstantiation : IEquatable<SolutionInstantiation>
    {
        public SolutionInstantiation(EA.Element element, EA.Element instance, bool selected = false) : this(element, instance.AsOption(), selected) { }

        public SolutionInstantiation(EA.Element element, Option<EA.Element> instance = null, bool selected = false)
        {
            Element = element;
            Instance = instance ?? Options.None<EA.Element>();
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

        public SolutionInstantiation Copy(EA.Element element = null, Option<EA.Element> instance = null, bool? selected = null)
        {
            return new SolutionInstantiation(element ?? Element, instance ?? Instance, selected ?? Selected);
        }
    }
}
