using AdAddIn.ADTechnology;
using EAAddInFramework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.PopulateDependencies
{
    using EdgeFilter = Func<EA.Element, EA.Element, EA.Connector, bool>;

    public static class DependencyTree
    {
        /// <summary>
        /// Performs a depth first search starting from <c>rootNode</c> and generates the according dependency tree.
        /// 
        /// The <c>edgeFilter</c> parameter may be used to specify an alternative traversal strategy. This function is called
        /// before the search algorithm decends to an ascending element. The algorithm only continous descending when
        /// <c>edgeFilter</c> returns true.
        /// </summary>
        /// <param name="rootNode">Start node that becomes the root of the dependency tree</param>
        /// <param name="edgeFilter">The strategy used to traverse the graph</param>
        /// <param name="levels">Maximum numbers of levels created in the dependency tree</param>
        public static LabeledTree<EA.Element, EA.Connector> Create(EA.Repository repo, EA.Element rootNode, EdgeFilter edgeFilter = null, int levels = 3)
        {
            return Create(repo, rootNode, edgeFilter ?? TraverseOnlyADConnectors, levels, ImmutableHashSet<String>.Empty);
        }

        private static LabeledTree<EA.Element, EA.Connector> Create(EA.Repository repo, EA.Element rootNode, EdgeFilter edgeFilter, int levels, IImmutableSet<String> visitedElementGuids)
        {
            var children = from c in rootNode.Connectors.Cast<EA.Connector>()
                           from child in DescendTo(repo, rootNode, c, edgeFilter, levels, visitedElementGuids.Add(rootNode.ElementGUID))
                           select child;
            return LabeledTree.Node<EA.Element, EA.Connector>(rootNode, children);
        }

        private static Option<LabeledTree<EA.Element, EA.Connector>.Edge> DescendTo(EA.Repository repo, EA.Element source, EA.Connector c, EdgeFilter edgeFilter, int levels, IImmutableSet<String> visitedElementGuids)
        {
            if (levels > 0)
            {
                var targetId = c.ClientID == source.ElementID ? c.SupplierID : c.ClientID;
                return from target in repo.TryGetElement(targetId)
                       where !visitedElementGuids.Contains(target.ElementGUID) && edgeFilter(source, target, c)
                       select LabeledTree.Edge(c, Create(repo, target, edgeFilter, levels - 1, visitedElementGuids));
            }
            else
            {
                return Options.None<LabeledTree<EA.Element, EA.Connector>.Edge>();
            }
        }

        /// <summary>
        /// Use this method as edge filter to create a dependency tree consisting of all reachable elements.
        /// </summary>
        public static bool TraverseAllConnectors(EA.Element from, EA.Element to, EA.Connector via)
        {
            return true;
        }

        /// <summary>
        /// Use this method as edge filter to create dependency trees that consists only of connectors as
        /// used in the AD domain.
        /// </summary>
        public static bool TraverseOnlyADConnectors(EA.Element from, EA.Element to, EA.Connector via)
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
