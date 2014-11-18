using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.PopulateDependencies
{
    public static class DependencyGraph
    {
        public static DirectedLabeledGraph<ModelEntity.Element, ModelEntity.Connector> Create(ModelEntityRepository repo, ModelEntity.Element rootNode,
            Func<ModelEntity.Element, ModelEntity.Connector, ModelEntity.Element, bool> edgeFilter)
        {
            return Create(repo, rootNode,
                new DirectedLabeledGraph<ModelEntity.Element, ModelEntity.Connector>(rootNode),
                edgeFilter);
        }

        private static DirectedLabeledGraph<ModelEntity.Element, ModelEntity.Connector> Create(
            ModelEntityRepository repo, 
            ModelEntity.Element source,
            DirectedLabeledGraph<ModelEntity.Element, ModelEntity.Connector> dependencyGraph,
            Func<ModelEntity.Element, ModelEntity.Connector, ModelEntity.Element, bool> edgeFilter)
        {
            var targets = from connector in source.Connectors
                          from target in connector.OppositeEnd(source, repo.GetElement)
                          where edgeFilter(source, connector, target)
                          select Tuple.Create(source, connector, target);

            return targets.Aggregate(dependencyGraph, (graph, edge) =>
            {
                var connected = graph.Connect(edge.Item1, edge.Item2, edge.Item3);
                if (graph.NodeLabels.Any(nl => nl.Guid == edge.Item3.Guid))
                {
                    return connected;
                }
                else
                {
                    return Create(repo, edge.Item3, connected, edgeFilter);
                }
            });
        }

        /// <summary>
        /// Use this method as edge filter to create a dependency tree consisting of all reachable elements.
        /// </summary>
        public static bool TraverseAllConnectors(ModelEntity.Element from, ModelEntity.Connector via, ModelEntity.Element to)
        {
            return true;
        }

        /// <summary>
        /// Use this method as edge filter to create dependency trees that consists only of connectors as
        /// used in the specified domain.
        /// </summary>
        public static Func<ModelEntity.Element, ModelEntity.Connector, ModelEntity.Element, bool> TraverseOnlyTechnologyConnectors(MDGTechnology technology)
        {
            var stypesByDirection = technology.ConnectorStereotypes.ToLookup(c => c.Direction.GetOrElse(Direction.Unspecified));

            var bidirectionalTypes = stypesByDirection[Direction.BiDirectional].Concat(stypesByDirection[Direction.Unspecified]);
            var forwardTypes = bidirectionalTypes.Concat(stypesByDirection[Direction.SourceToDestination]);
            var backwardTypes = bidirectionalTypes.Concat(stypesByDirection[Direction.DestinationToSource]);

            return (from, via, to) =>
            {
                return (forwardTypes.Any(stype => via.Is(stype)) && via.EaObject.ClientID == from.Id)
                     || (backwardTypes.Any(stype => via.Is(stype)) && via.EaObject.SupplierID == from.Id);
            };
        }
    }
}
