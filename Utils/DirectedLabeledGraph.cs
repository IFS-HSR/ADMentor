using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace Utils
{
    public class DirectedLabeledGraph<NodeLabel, EdgeLabel>
    {
        private readonly Func<NodeLabel, NodeLabel, bool> CompareNodes;

        private readonly Func<EdgeLabel, EdgeLabel, bool> CompareEdges;

        public DirectedLabeledGraph(Func<NodeLabel, NodeLabel, bool> compareNodes = null, Func<EdgeLabel, EdgeLabel, bool> compareEdges = null)
            : this(ImmutableList.Create<Node>(), compareNodes, compareEdges) { }

        internal DirectedLabeledGraph(IImmutableList<Node> nodes, Func<NodeLabel, NodeLabel, bool> compareNodes = null, Func<EdgeLabel, EdgeLabel, bool> compareEdges = null)
        {
            Nodes = nodes;
            CompareNodes = compareNodes ?? ((a, b) => a.Equals(b));
            CompareEdges = compareEdges ?? ((a, b) => a.Equals(b));
        }

        internal IImmutableList<Node> Nodes { get; private set; }

        public IEnumerable<NodeLabel> NodeLabels
        {
            get { return Nodes.Select(n => n.Label); }
        }

        public IEnumerable<Tuple<NodeLabel, EdgeLabel, NodeLabel>> Edges
        {
            get
            {
                return from source in Nodes
                       from edge in source.Edges
                       select Tuple.Create(source.Label, edge.Label, edge.TargetLabel);
            }
        }

        internal class Node
        {
            internal Node(NodeLabel label, IEnumerable<Edge> edges = null)
            {
                Label = label;
                Edges = (edges ?? new List<Edge>()).ToList();
            }

            internal NodeLabel Label { get; private set; }

            internal IEnumerable<Edge> Edges { get; private set; }

            public override string ToString()
            {
                return String.Format("Node({0}, {1})",
                    Label,
                    Edges.Select(e => e.ToString()).Join(", "));
            }
        }

        internal class Edge
        {
            internal Edge(EdgeLabel label, NodeLabel target)
            {
                Label = label;
                TargetLabel = target;
            }

            internal EdgeLabel Label { get; private set; }

            internal NodeLabel TargetLabel { get; private set; }

            public override string ToString()
            {
                return String.Format("Edge({0}, {1})", Label, TargetLabel);
            }
        }

        private class NodeEqualityComparer : IEqualityComparer<Node>
        {
            private readonly Func<NodeLabel, NodeLabel, bool> CompareNodes;

            public NodeEqualityComparer(Func<NodeLabel, NodeLabel, bool> compareNodes)
            {
                CompareNodes = compareNodes;
            }

            public bool Equals(Node x, Node y)
            {
                return CompareNodes(x.Label, y.Label);
            }

            public int GetHashCode(Node obj)
            {
                return obj.Label.GetHashCode();
            }
        }

        public void TraverseEdgesBF(NodeLabel startLabel, Action<NodeLabel, EdgeLabel, NodeLabel> act)
        {
            var startNode = Nodes.FirstOption(n => CompareNodes(n.Label, startLabel)).Value;
            var visitedNodes = new HashSet<Node>(new NodeEqualityComparer(CompareNodes));
            TraverseEdgesBF(startNode, act, visitedNodes);
        }

        private void TraverseEdgesBF(Node source, Action<NodeLabel, EdgeLabel, NodeLabel> act, ISet<Node> visitedNodes)
        {
            visitedNodes.Add(source);
            source.Edges.ForEach(edge =>
            {
                act(source.Label, edge.Label, edge.TargetLabel);
                var targetNode = Nodes.First(n => CompareNodes(n.Label, edge.TargetLabel));
                if (!visitedNodes.Contains(targetNode))
                {
                    TraverseEdgesBF(targetNode, act, visitedNodes);
                }
            });
        }

        public DirectedLabeledGraph<N, EdgeLabel> MapNodeLabels<N>(Func<NodeLabel, N> fn, Func<N, N, bool> compareNodes = null)
        {
            var mapped = Nodes.ToImmutableDictionary(n => n.Label, n => fn(n.Label));

            var mappedGraph = new DirectedLabeledGraph<N, EdgeLabel>(compareNodes, CompareEdges);

            Edges.ForEach(e =>
            {
                mappedGraph = mappedGraph.Connect(mapped[e.Item1], e.Item2, mapped[e.Item3]);
            });

            return mappedGraph;
        }

        public DirectedLabeledGraph<NodeLabel, EdgeLabel> Connect(NodeLabel sourceLabel, EdgeLabel edgeLabel, NodeLabel targetLabel)
        {
            var target = Nodes.FirstOption(n => CompareNodes(n.Label, targetLabel)).GetOrElse(new Node(targetLabel));
            var edge = new Edge(edgeLabel, targetLabel);
            var source = Nodes.FirstOption(n => CompareNodes(n.Label, sourceLabel))
                .GetOrElse(new Node(sourceLabel));

            if (!source.Edges.Any(e => CompareEdges(e.Label, edgeLabel)))
            {
                var newSource = new Node(source.Label, source.Edges.Concat(new[] { edge }));
                var newNodes = Nodes.RemoveAll(n => CompareNodes(n.Label, sourceLabel) || CompareNodes(n.Label, targetLabel));

                return new DirectedLabeledGraph<NodeLabel, EdgeLabel>(newNodes.Add(newSource).Add(target), CompareNodes, CompareEdges);
            }
            else
            {
                return this;
            }
        }
    }

    public static class DirectedLabeledGraph
    {
        public static DirectedLabeledGraph<N, E> Create<N, E>(Func<N, N, bool> compareNodes, Func<E, E, bool> compareEdges, params Tuple<N, E, N>[] edges)
        {
            return edges.Aggregate(new DirectedLabeledGraph<N, E>(compareNodes, compareEdges), (graph, edge) =>
            {
                return graph.Connect(edge.Item1, edge.Item2, edge.Item3);
            });
        }

        public static DirectedLabeledGraph<N, E> Create<N, E>(params Tuple<N, E, N>[] edges)
        {
            return Create(null, null, edges);
        }
    }
}
