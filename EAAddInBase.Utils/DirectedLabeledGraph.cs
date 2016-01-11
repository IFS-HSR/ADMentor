using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace EAAddInBase.Utils
{
    public sealed class DirectedLabeledGraph<NodeLabel, EdgeLabel>
    {
        private readonly IEqualityComparer<NodeLabel> NodeComparer;

        private readonly IEqualityComparer<EdgeLabel> EdgeComparer;

        public DirectedLabeledGraph(NodeLabel startLabel, IEqualityComparer<NodeLabel> nodeComparer = null, IEqualityComparer<EdgeLabel> edgeComparer = null)
            : this(startLabel, ImmutableList.Create<Node>(new Node(startLabel)), nodeComparer, edgeComparer) { }

        internal DirectedLabeledGraph(
            NodeLabel startLabel,
            IImmutableList<Node> nodes,
            IEqualityComparer<NodeLabel> nodeComparer = null,
            IEqualityComparer<EdgeLabel> edgeComparer = null)
        {
            StartLabel = startLabel;
            Nodes = nodes;
            NodeComparer = nodeComparer ?? EqualityComparer<NodeLabel>.Default;
            EdgeComparer = edgeComparer ?? EqualityComparer<EdgeLabel>.Default;
        }

        internal IImmutableList<Node> Nodes { get; private set; }

        public NodeLabel StartLabel { get; private set; }

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

        public void TraverseEdgesBF(Action<NodeLabel, EdgeLabel, NodeLabel> act)
        {
            AggregateEdges(Unit.Instance, (acc, from, via, to) =>
            {
                act(from, via, to);
                return acc;
            });
        }

        public Acc AggregateEdges<Acc>(Acc seed, Func<Acc, NodeLabel, EdgeLabel, NodeLabel, Acc> fn)
        {
            var startNode = Nodes.FirstOption(n => NodeComparer.Equals(n.Label, StartLabel)).Value;
            var visitedNodes = new HashSet<NodeLabel>(NodeComparer);
            return AggregateEdges(startNode, seed, fn, visitedNodes);
        }

        private Acc AggregateEdges<Acc>(Node source, Acc seed, Func<Acc, NodeLabel, EdgeLabel, NodeLabel, Acc> fn, HashSet<NodeLabel> visitedNodes)
        {
            visitedNodes.Add(source.Label);
            return source.Edges.Aggregate(seed, (acc, edge) =>
            {
                var res = fn(acc, source.Label, edge.Label, edge.TargetLabel);
                var targetNode = Nodes.First(n => NodeComparer.Equals(n.Label, edge.TargetLabel));
                if (!visitedNodes.Contains(targetNode.Label))
                {
                    return AggregateEdges(targetNode, res, fn, visitedNodes);
                }
                else
                {
                    return res;
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

            return Edges.Aggregate(new DirectedLabeledGraph<N, EdgeLabel>(mappedNodes[StartLabel], nodeComparer, EdgeComparer), (graph, edge) =>
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

                return new DirectedLabeledGraph<NodeLabel, EdgeLabel>(StartLabel, newNodes.Add(newSource).Add(target), NodeComparer, EdgeComparer);
            }
            else
            {
                return this;
            }
        }

        public LabeledTree<NodeLabel, EdgeLabel> ToTree(Func<NodeLabel, EdgeLabel, NodeLabel, bool> edgeFilter = null)
        {
            var filter = edgeFilter ?? DirectedLabeledGraph.AllEdges<NodeLabel, EdgeLabel>;
            var rootNode = Nodes.FirstOption(n => NodeComparer.Equals(n.Label, StartLabel)).Value;
            var visitedNodes = new HashSet<NodeLabel>(NodeComparer);
            return ToTree(rootNode, visitedNodes, filter);
        }

        private LabeledTree<NodeLabel, EdgeLabel> ToTree(Node rootNode, ISet<NodeLabel> visitedNodes,
            Func<NodeLabel, EdgeLabel, NodeLabel, bool> edgeFilter)
        {
            if (visitedNodes.Contains(rootNode.Label))
            {
                return LabeledTree.Node<NodeLabel, EdgeLabel>(rootNode.Label);
            }
            else
            {
                visitedNodes.Add(rootNode.Label);
                var edges = from e in rootNode.Edges
                            where edgeFilter(rootNode.Label, e.Label, e.TargetLabel)
                            from target in Nodes.FirstOption(n => NodeComparer.Equals(n.Label, e.TargetLabel))
                            select LabeledTree.Edge(e.Label, ToTree(target, visitedNodes, edgeFilter));
                return LabeledTree.Node(rootNode.Label, edges);
            }
        }

        public bool IsLeaf(NodeLabel n)
        {
            return !Edges.Any(edge => NodeComparer.Equals(edge.Item1, n));
        }
    }

    public static class DirectedLabeledGraph
    {
        public static DirectedLabeledGraph<N, E> Create<N, E>(IEqualityComparer<N> nodeComparer,
            IEqualityComparer<E> edgeComparer, params Tuple<N, E, N>[] edges)
        {
            return edges.Aggregate(new DirectedLabeledGraph<N, E>(edges.First().Item1, nodeComparer, edgeComparer), (graph, edge) =>
            {
                return graph.Connect(edge.Item1, edge.Item2, edge.Item3);
            });
        }

        public static DirectedLabeledGraph<N, E> Create<N, E>(params Tuple<N, E, N>[] edges)
        {
            return Create(null, null, edges);
        }

        public static bool AllEdges<N, E>(N from, E via, N to)
        {
            return true;
        }

        public static Func<object, E, object, bool> TraverseEdgeOnlyOnce<E>(IEqualityComparer<E> edgeComparer = null)
        {
            edgeComparer = edgeComparer ?? EqualityComparer<E>.Default;
            var visitedEdges = new HashSet<E>(edgeComparer);
            return (from, via, to) =>
            {
                if (visitedEdges.Contains(via))
                {
                    return false;
                }
                else
                {
                    visitedEdges.Add(via);
                    return true;
                }
            };
        }
    }
}
