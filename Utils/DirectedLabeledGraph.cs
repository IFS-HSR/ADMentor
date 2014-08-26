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
        private readonly IEqualityComparer<NodeLabel> NodeComparer;

        private readonly IEqualityComparer<EdgeLabel> EdgeComparer;

        public DirectedLabeledGraph(IEqualityComparer<NodeLabel> nodeComparer = null, IEqualityComparer<EdgeLabel> edgeComparer = null)
            : this(ImmutableList.Create<Node>(), nodeComparer, edgeComparer) { }

        internal DirectedLabeledGraph(IImmutableList<Node> nodes,
            IEqualityComparer<NodeLabel> nodeComparer = null,
            IEqualityComparer<EdgeLabel> edgeComparer = null)
        {
            Nodes = nodes;
            NodeComparer = nodeComparer ?? EqualityComparer<NodeLabel>.Default;
            EdgeComparer = edgeComparer ?? EqualityComparer<EdgeLabel>.Default;
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
            private readonly IEqualityComparer<NodeLabel> NodeComparer;

            public NodeEqualityComparer(IEqualityComparer<NodeLabel> nodeComparer)
            {
                NodeComparer = nodeComparer;
            }

            public bool Equals(Node x, Node y)
            {
                return NodeComparer.Equals(x.Label, y.Label);
            }

            public int GetHashCode(Node obj)
            {
                return NodeComparer.GetHashCode(obj.Label);
            }
        }

        public void TraverseEdgesBF(NodeLabel startLabel, Action<NodeLabel, EdgeLabel, NodeLabel> act)
        {
            var startNode = Nodes.FirstOption(n => NodeComparer.Equals(n.Label, startLabel)).Value;
            var visitedNodes = new HashSet<Node>(new NodeEqualityComparer(NodeComparer));
            TraverseEdgesBF(startNode, act, visitedNodes);
        }

        private void TraverseEdgesBF(Node source, Action<NodeLabel, EdgeLabel, NodeLabel> act, ISet<Node> visitedNodes)
        {
            visitedNodes.Add(source);
            source.Edges.ForEach(edge =>
            {
                act(source.Label, edge.Label, edge.TargetLabel);
                var targetNode = Nodes.First(n => NodeComparer.Equals(n.Label, edge.TargetLabel));
                if (!visitedNodes.Contains(targetNode))
                {
                    TraverseEdgesBF(targetNode, act, visitedNodes);
                }
            });
        }

        public DirectedLabeledGraph<NodeLabel, EdgeLabel> MapNodeLabels(Func<NodeLabel, NodeLabel> fn)
        {
            return MapNodeLabels<NodeLabel>(fn, NodeComparer);
        }

        public DirectedLabeledGraph<N, EdgeLabel> MapNodeLabels<N>(Func<NodeLabel, N> fn, IEqualityComparer<N> nodeComparer = null)
        {
            var mappedNodes = Nodes.ToImmutableDictionary(n => n.Label, n => fn(n.Label), NodeComparer);

            return Edges.Aggregate(new DirectedLabeledGraph<N, EdgeLabel>(nodeComparer, EdgeComparer), (graph, edge) =>
            {
                return graph.Connect(mappedNodes[edge.Item1], edge.Item2, mappedNodes[edge.Item3]);
            });
        }

        public DirectedLabeledGraph<NodeLabel, EdgeLabel> Connect(NodeLabel sourceLabel, EdgeLabel edgeLabel, NodeLabel targetLabel)
        {
            var target = Nodes.FirstOption(n => NodeComparer.Equals(n.Label, targetLabel))
                .GetOrElse(new Node(targetLabel));
            var edge = new Edge(edgeLabel, targetLabel);
            var source = Nodes.FirstOption(n => NodeComparer.Equals(n.Label, sourceLabel))
                .GetOrElse(new Node(sourceLabel));

            if (!source.Edges.Any(e => EdgeComparer.Equals(e.Label, edgeLabel)))
            {
                var newSource = new Node(source.Label, source.Edges.Concat(new[] { edge }));
                var newNodes = Nodes.RemoveAll(n => NodeComparer.Equals(n.Label, sourceLabel) || NodeComparer.Equals(n.Label, targetLabel));

                return new DirectedLabeledGraph<NodeLabel, EdgeLabel>(newNodes.Add(newSource).Add(target), NodeComparer, EdgeComparer);
            }
            else
            {
                return this;
            }
        }

        public DirectedLabeledGraph<NodeLabel, EdgeLabel> AddNode(NodeLabel nodeLabel)
        {
            return Nodes.FirstOption(n => NodeComparer.Equals(n.Label, nodeLabel)).Match(
                node =>
                {
                    return this;
                },
                () =>
                {
                    var newNode = new Node(nodeLabel);
                    return new DirectedLabeledGraph<NodeLabel, EdgeLabel>(
                        Nodes.Add(newNode), NodeComparer, EdgeComparer);
                });
        }
    }

    public static class DirectedLabeledGraph
    {
        public static DirectedLabeledGraph<N, E> Create<N, E>(IEqualityComparer<N> nodeComparer,
            IEqualityComparer<E> edgeComparer, params Tuple<N, E, N>[] edges)
        {
            return edges.Aggregate(new DirectedLabeledGraph<N, E>(nodeComparer, edgeComparer), (graph, edge) =>
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
