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

    public class DependencyTreeNode
    {
        public DependencyTreeNode(EA.Element element, IEnumerable<DependencyTreeEdge> children)
        {
            Element = element;
            Children = children;
        }

        public EA.Element Element { get; private set; }

        public IEnumerable<DependencyTreeEdge> Children { get; private set; }
    }

    public class DependencyTreeEdge
    {
        public DependencyTreeEdge(EA.Connector connector, DependencyTreeNode node)
        {
            Connector = connector;
            Node = node;
        }

        public EA.Connector Connector { get; private set; }

        public DependencyTreeNode Node { get; private set; }
    }

    public static class DependencyTree
    {
        public static DependencyTreeNode Create(EA.Repository repo, EA.Element rootNode, EdgeFilter edgeFilter = null, int levels = 3)
        {
            return Create(repo, rootNode, edgeFilter ?? UseOnlyADConnectors, levels, ImmutableHashSet<String>.Empty);
        }

        public static bool UseOnlyADConnectors(EA.Element from, EA.Element to, EA.Connector via)
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

        public static bool UseAllConnectors(EA.Element from, EA.Element to, EA.Connector via)
        {
            return true;
        }

        private static DependencyTreeNode Create(EA.Repository repo, EA.Element rootNode, EdgeFilter edgeFilter, int levels, IImmutableSet<String> visitedElementGuids)
        {
            var children = from c in rootNode.Connectors.Cast<EA.Connector>()
                           from child in DescendTo(repo, rootNode, c, edgeFilter, levels, visitedElementGuids.Add(rootNode.ElementGUID))
                           select child;
            return new DependencyTreeNode(rootNode, children);
        }

        private static Option<DependencyTreeEdge> DescendTo(EA.Repository repo, EA.Element source, EA.Connector c, EdgeFilter edgeFilter, int levels, IImmutableSet<String> visitedElementGuids)
        {
            if (levels > 0)
            {
                var targetId = c.ClientID == source.ElementID ? c.SupplierID : c.ClientID;
                return from target in repo.TryGetElement(targetId)
                       where !visitedElementGuids.Contains(target.ElementGUID) && edgeFilter(source, target, c)
                       select new DependencyTreeEdge(c, Create(repo, target, edgeFilter, levels - 1, visitedElementGuids));
            }
            else
            {
                return Options.None<DependencyTreeEdge>();
            }
        }
    }
}
