using AdAddIn.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using EAAddInFramework;
using AdAddIn.ADTechnology;
using EAAddInFramework.DataAccess;

namespace AdAddIn.PopulateDependencies
{
    public class SolutionInstantiationGraph
    {
        private readonly ModelEntityRepository Repo;

        private SolutionInstantiationGraph(ModelEntityRepository repo, DirectedLabeledGraph<ElementInstantiation, ModelEntity.Connector> graph)
        {
            Repo = repo;
            Graph = graph;
        }

        public DirectedLabeledGraph<ElementInstantiation, ModelEntity.Connector> Graph { get; private set; }

        private static Func<ModelEntity.Element, ModelEntity.Connector, ModelEntity.Element, bool> DependencyGraphFilter =
            DependencyGraph.TraverseOnlyTechnologyConnectors(ADTechnology.Technologies.AD);

        public static Option<SolutionInstantiationGraph> Create(ModelEntityRepository repo, SolutionEntity solutionItem)
        {
            return from classifier in solutionItem.GetClassifier(repo.GetElement)
                   from problemSpaceEntity in classifier.Match<ProblemSpaceEntity>()
                   let problemSpace = DependencyGraph.Create(repo, problemSpaceEntity, DependencyGraphFilter)
                   let solution = DependencyGraph.Create(repo, solutionItem, DependencyGraphFilter)
                   select new SolutionInstantiationGraph(repo, Compare(problemSpace, solution));
        }

        private static DirectedLabeledGraph<ElementInstantiation, ModelEntity.Connector> Compare(
            DirectedLabeledGraph<ModelEntity.Element, ModelEntity.Connector> problemSpace,
            DirectedLabeledGraph<ModelEntity.Element, ModelEntity.Connector> solution)
        {
            return problemSpace.MapNodeLabels<ElementInstantiation>(problemItem =>
            {
                var instance = solution.NodeLabels.FirstOption(solutionItem => solutionItem.EaObject.ClassifierID == problemItem.Id);
                return new ElementInstantiation(problemItem, instance);
            });
        }

        public SolutionInstantiationGraph InstantiateSelectedItems(ModelEntity.Package package)
        {
            var newGraph = Graph.MapNodeLabels(problemItem =>
            {
                if (problemItem.Selected)
                    return problemItem.CreateInstanceIfMissing(Repo, package);
                else
                    return problemItem;
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
                        edge.GetStereotype(ADTechnology.Technologies.AD.ConnectorStereotypes).Do(stype =>
                        {
                            var connectsAlternativeToProblem =
                                stype == ConnectorStereotypes.HasAlternative && solutionSource is OptionOccurrence;
                            var alreadyExisting = solutionSource.Connectors().Any(c =>
                            {
                                return c.Is(stype) && (c.EaObject.SupplierID == solutionTarget.Id || c.EaObject.ClientID == solutionTarget.Id);
                            });

                            if (!connectsAlternativeToProblem && !alreadyExisting)
                            {
                                Repo.Connect(solutionSource, solutionTarget, stype);
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
                var selected = nodes.Any(n => n.Element.Guid == si.Element.Guid && n.Selected);
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
        public Unit CreateDiagramElements(ModelEntity.Diagram diagram)
        {
            var siblings = new Dictionary<ElementInstantiation, int>();

            Graph.TraverseEdgesBF((from, via, to) =>
            {
                var leftHandSiblings = siblings.ContainsKey(from) ? siblings[from] : 0;

                to.Instance.Do(toInstance =>
                {
                    if (!diagram.GetObject(toInstance).IsDefined)
                    {
                        from.Instance.Do(fromInstance =>
                        {
                            var parentObject = diagram.GetObject(fromInstance).Value;

                            var verticalOffset = leftHandSiblings * 110 - 40;
                            var horizontalOffset = -200 - leftHandSiblings * 20;

                            diagram.AddObject(toInstance,
                                left: parentObject.EaObject.left + verticalOffset, right: parentObject.EaObject.right + verticalOffset,
                                top: parentObject.EaObject.top + horizontalOffset, bottom: parentObject.EaObject.bottom + horizontalOffset);

                            siblings[from] = leftHandSiblings + 1;
                        });
                    }
                });
            });
            Repo.Reload(diagram);
            Repo.Save(diagram);

            return Unit.Instance;
        }

        /// <summary>
        /// A helper method to create solution instantiation graphs with the correct equality comparers.
        /// </summary>
        /// <param name="edges"></param>
        /// <returns></returns>
        public static SolutionInstantiationGraph Create(
            params Tuple<ElementInstantiation, ModelEntity.Connector, ElementInstantiation>[] edges)
        {
            return new SolutionInstantiationGraph(null, DirectedLabeledGraph.Create(
                EqualityComparer<ElementInstantiation>.Default,
                EqualityComparer<ModelEntity.Connector>.Default,
                edges));
        }
    }
}
