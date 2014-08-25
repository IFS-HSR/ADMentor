using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using EAAddInFramework;
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
        public static bool CompareElements(EA.Element a, EA.Element b)
        {
            return a.ElementGUID == b.ElementGUID;
        }

        public static bool CompareConnectors(EA.Connector a, EA.Connector b)
        {
            return a.ConnectorGUID == b.ConnectorGUID;
        }

        public static DirectedLabeledGraph<EA.Element, EA.Connector> Create(ElementRepository repo, EA.Element rootNode,
            Func<EA.Element, EA.Connector, EA.Element, bool> edgeFilter)
        {
            return Create(repo, rootNode, new DirectedLabeledGraph<EA.Element, EA.Connector>(), edgeFilter);
        }

        private static DirectedLabeledGraph<EA.Element, EA.Connector> Create(ElementRepository repo, EA.Element source,
            DirectedLabeledGraph<EA.Element, EA.Connector> dependencyGraph,
            Func<EA.Element, EA.Connector, EA.Element, bool> edgeFilter)
        {
            var targets = from c in source.Connectors.Cast<EA.Connector>()
                          let target = OppositeEnd(repo, source, c)
                          where edgeFilter(source, c, target)
                          select Tuple.Create(source, c, target);

            return targets.Aggregate(dependencyGraph, (graph, edge) =>
            {
                var connected = graph.Connect(edge.Item1, edge.Item2, edge.Item3);
                if (graph.NodeLabels.Any(nl => CompareElements(nl, edge.Item3)))
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
        /// used in the AD domain.
        /// </summary>
        public static bool TraverseOnlyADConnectors(EA.Element from, EA.Connector via, EA.Element to)
        {
            var directedConnectors = new[] {
                    ConnectorStereotypes.HasAlternative,
                    ConnectorStereotypes.Raises,
                    ConnectorStereotypes.Includes,
                    ConnectorStereotypes.Overrides,
                    ConnectorStereotypes.Supports
                };

            var undirectedConnectors = new[]{
                    ConnectorStereotypes.BoundTo,
                    ConnectorStereotypes.ConflictsWith
                };

            return (directedConnectors.Concat(undirectedConnectors).Any(stereotype => via.Is(stereotype)) && via.ClientID == from.ElementID) ||
                (undirectedConnectors.Any(stereotype => via.Is(stereotype)) && via.SupplierID == from.ElementID);
        }
    }
}
