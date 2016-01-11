using ADMentor.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;
using EAAddInBase;
using ADMentor.ADTechnology;
using EAAddInBase.DataAccess;
using System.Threading;

namespace ADMentor.PopulateDependencies
{
    sealed class SolutionInstantiationGraph
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
                   from problemSpaceEntity in classifier.TryCast<ProblemSpaceEntity>()
                   let problemSpace = DependencyGraph.Create(repo, problemSpaceEntity, DependencyGraphFilter)
                   let solution = DependencyGraph.Create(repo, solutionItem, DependencyGraphFilter)
                   select new SolutionInstantiationGraph(repo, Merge(problemSpace, solution));
        }

        private static DirectedLabeledGraph<ElementInstantiation, ModelEntity.Connector> Merge(
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
                var connectorData = from connectorSource in source.Instance
                                    from connectorTarget in target.Instance
                                    from stype in edge.GetStereotype(ADTechnology.Technologies.AD.ConnectorStereotypes)
                                    select Tuple.Create(connectorSource, connectorTarget, stype);

                connectorData.ForEach((connectorSource, connectorTarget, stype) =>
                {
                    var connectsAlternativeToProblem =
                        stype == ConnectorStereotypes.AddressedBy && connectorSource is OptionOccurrence;
                    var alreadyExisting = connectorSource.Connectors.Any(c =>
                    {
                        return c.Is(stype) && (c.EaObject.SupplierID == connectorTarget.Id || c.EaObject.ClientID == connectorTarget.Id);
                    });

                    if (!connectsAlternativeToProblem && !alreadyExisting)
                    {
                        Repo.Connect(connectorSource, connectorTarget, stype);
                    }
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
            var tree = Graph.ToTree(DirectedLabeledGraph.TraverseEdgeOnlyOnce<ModelEntity.Connector>());
            var rootObject = diagram.GetObject(tree.Label.Instance.Value).Value;
            var vNull = Convert.ToInt32(
                rootObject.EaObject.left - ((tree.Leafs() - 1) / 2d) * 120);
            var width = rootObject.EaObject.right - rootObject.EaObject.left;
            var hNull = rootObject.EaObject.top;
            var height = rootObject.EaObject.top - rootObject.EaObject.bottom;

            if (vNull < 10)
            {
                vNull = 10;
            }

            Graph.TraverseEdgesBF((from, via, to) =>
            {
                var objectData = from childInstance in to.Instance
                                 where !diagram.GetObject(childInstance).IsDefined
                                 from parentInstance in @from.Instance
                                 from parentObject in diagram.GetObject(parentInstance)
                                 select Tuple.Create(parentObject, childInstance);

                objectData.ForEach((parentObject, childInstance) =>
                {
                    var lhl = tree.LeftHandLeafs(to);
                    var l = tree.Leafs(to);
                    var level = tree.Level(to);
                    var vOffset = Convert.ToInt32(
                        vNull + (lhl + (l - 1) / 2d) * 120);
                    var hOffset = hNull - (150 * level);

                    diagram.AddObject(childInstance.Id,
                        left: vOffset, right: vOffset + width,
                        top: hOffset, bottom: hOffset - height);
                });
            });
            Repo.Reload(diagram);

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
