using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInBase.Utils
{
    /// <summary>
    /// A tree with labels of different types for nodes and edges.
    /// </summary>
    /// <typeparam name="N">Node Labels</typeparam>
    /// <typeparam name="E">Edge Labels</typeparam>
    public sealed class LabeledTree<N, E>
    {
        public LabeledTree(N label, IImmutableList<Edge> edges)
        {
            Label = label;
            Edges = edges;
        }

        public N Label { get; private set; }

        public IImmutableList<Edge> Edges { get; private set; }

        public IEnumerable<N> NodeLabels
        {
            get
            {
                return new[] { Label }.Concat(from e in Edges from nl in e.Target.NodeLabels select nl);
            }
        }

        public override string ToString()
        {
            return ToString(0);
        }

        internal string ToString(int level)
        {
            return String.Format("{0}{1}", new String(' ', 2 * level), Label.ToString());
        }

        public class Edge
        {
            public Edge(E label, LabeledTree<N, E> target)
            {
                Label = label;
                Target = target;
            }

            public E Label { get; private set; }

            public LabeledTree<N, E> Target { get; private set; }
        }

        public LabeledTree<N, E> TransformTopDown(Func<N, E, N, N> fn)
        {
            var edges = from edge in Edges
                        let newTarget = fn(Label, edge.Label, edge.Target.Label)
                        select LabeledTree.Edge(edge.Label, LabeledTree.Node(newTarget, edge.Target.Edges).TransformTopDown(fn));
            return LabeledTree.Node(Label, edges);
        }

        public void TraverseTopDown(Action<N, E, N> fn)
        {
            Edges.ForEach(edge =>
            {
                fn(Label, edge.Label, edge.Target.Label);
                edge.Target.TraverseTopDown(fn);
            });
        }

        public int Leafs()
        {
            if (Edges.Count == 0)
                return 1;
            else
                return AggregateDF(0, (acc, edge) =>
                {
                    if (edge.Target.Edges.Count == 0)
                    {
                        return acc + 1;
                    }
                    else
                    {
                        return acc;
                    }
                });
        }

        public int Leafs(N node)
        {
            return SubTree(node).Fold(tree => tree.Leafs(), () => 0);
        }

        public Option<LabeledTree<N, E>> SubTree(N node)
        {
            if (Label.Equals(node))
                return Options.Some(this);
            else
                return (from e in Edges
                        from s in e.Target.SubTree(node)
                        select s).FirstOption();
        }

        public int LeftHandLeafs(N node)
        {
            if (Label.Equals(node))
                return 0;
            else
                return AggregateDF(Tuple.Create(0, false), (acc, edge) =>
                {
                    if (!acc.Item2 && edge.Target.Label.Equals(node))
                    {
                        return Tuple.Create(acc.Item1, true);
                    }
                    else if (!acc.Item2 && edge.Target.Edges.Count == 0)
                    {
                        return Tuple.Create(acc.Item1 + 1, acc.Item2);
                    }
                    else
                    {
                        return acc;
                    }
                }).Item1;
        }

        private Acc AggregateDF<Acc>(Acc seed, Func<Acc, Edge, Acc> fn)
        {
            return Edges.Aggregate(seed, (acc, e) =>
            {
                return e.Target.AggregateDF(fn(acc, e), fn);
            });
        }

        public int Level(N node)
        {
            if (Label.Equals(node))
            {
                return 0;
            }
            else if (Edges.Count == 0)
            {
                return int.MaxValue;
            }
            else
            {
                var min = Edges.Min(e => e.Target.Level(node));
                return min == int.MaxValue ? min : min + 1;
            }
        }
    }

    public static class LabeledTree
    {
        public static LabeledTree<N, E> Node<N, E>(N label, IEnumerable<LabeledTree<N, E>.Edge> edges)
        {
            return new LabeledTree<N, E>(label, edges.ToImmutableList());
        }

        public static LabeledTree<N, E> Node<N, E>(N label, params LabeledTree<N, E>.Edge[] edges)
        {
            return new LabeledTree<N, E>(label, edges.ToImmutableList());
        }

        public static LabeledTree<N, E>.Edge Edge<N, E>(E label, LabeledTree<N, E> target)
        {
            return new LabeledTree<N, E>.Edge(label, target);
        }
    }
}
