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
    public static class DependencyGraph
    {
        public static DirectedLabeledGraph<EA.Element, EA.Connector> Create(ElementRepository repo, EA.Element rootNode,
            Func<EA.Element, EA.Connector, EA.Element, bool> edgeFilter)
        {
            return Create(repo, rootNode,
                new DirectedLabeledGraph<EA.Element, EA.Connector>(rootNode, new ElementComparer(), new ConnectorComparer()),
                edgeFilter);
        }

        private static DirectedLabeledGraph<EA.Element, EA.Connector> Create(ElementRepository repo, EA.Element source,
            DirectedLabeledGraph<EA.Element, EA.Connector> dependencyGraph,
            Func<EA.Element, EA.Connector, EA.Element, bool> edgeFilter)
        {
            var targets = from connector in source.Connectors.Cast<EA.Connector>()
                          let target = OppositeEnd(repo, source, connector)
                          where edgeFilter(source, connector, target)
                          select Tuple.Create(source, connector, target);

            return targets.Aggregate(dependencyGraph, (graph, edge) =>
            {
                var connected = graph.Connect(edge.Item1, edge.Item2, edge.Item3);
                if (graph.NodeLabels.Any(nl => nl.ElementGUID == edge.Item3.ElementGUID))
                {
                    return connected;
                }
                else
                {
                    return Create(repo, edge.Item3, connected, edgeFilter);
                }
            });
        }

        private static EA.Element OppositeEnd(ElementRepository repo, EA.Element sourceNode, EA.Connector c)
        {
            if (c.ClientID == sourceNode.ElementID)
            {
                return repo.GetElement(c.SupplierID).Value;
            }
            else
            {
                return repo.GetElement(c.ClientID).Value;
            }
        }

        /// <summary>
        /// Use this method as edge filter to create a dependency tree consisting of all reachable elements.
        /// </summary>
        public static bool TraverseAllConnectors(EA.Element from, EA.Connector via, EA.Element to)
        {
            return true;
        }

        /// <summary>
        /// Use this method as edge filter to create dependency trees that consists only of connectors as
        /// used in the specified domain.
        /// </summary>
        public static Func<EA.Element, EA.Connector, EA.Element, bool> TraverseOnlyTechnologyConnectors(MDGTechnology technology)
        {
            var stypesByDirection = technology.ConnectorStereotypes.ToLookup(c => c.Direction.GetOrElse(Direction.Unspecified));

            var bidirectionalTypes = stypesByDirection[Direction.BiDirectional].Concat(stypesByDirection[Direction.Unspecified]);
            var forwardTypes = bidirectionalTypes.Concat(stypesByDirection[Direction.SourceToDestination]);
            var backwardTypes = bidirectionalTypes.Concat(stypesByDirection[Direction.DestinationToSource]);

            return (from, via, to) =>
            {
                return (forwardTypes.Any(stype => via.Is(stype)) && via.ClientID == from.ElementID)
                     || (backwardTypes.Any(stype => via.Is(stype)) && via.SupplierID == from.ElementID);
            };
        }

        public class ElementComparer : IEqualityComparer<EA.Element>
        {
            public bool Equals(EA.Element x, EA.Element y)
            {
                return x.ElementGUID == y.ElementGUID;
            }

            public int GetHashCode(EA.Element obj)
            {
                return obj.ElementGUID.GetHashCode();
            }
        }

        public class ConnectorComparer : IEqualityComparer<EA.Connector>
        {
            public bool Equals(EA.Connector x, EA.Connector y)
            {
                return x.ConnectorGUID == y.ConnectorGUID;
            }

            public int GetHashCode(EA.Connector obj)
            {
                return obj.ConnectorGUID.GetHashCode();
            }
        }
    }
}
