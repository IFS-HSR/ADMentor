using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    /// <summary>
    /// A tree with labels of different types for nodes and edges.
    /// </summary>
    /// <typeparam name="N">Node Labels</typeparam>
    /// <typeparam name="E">Edge Labels</typeparam>
    public class LabeledTree<N, E>
    {
        public LabeledTree(N label, IEnumerable<Edge> edges)
        {
            Label = label;
            Edges = edges;
        }

        public N Label { get; private set; }

        public IEnumerable<Edge> Edges { get; private set; }

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
    }

    public static class LabeledTree
    {
        public static LabeledTree<N, E> Node<N, E>(N label, IEnumerable<LabeledTree<N, E>.Edge> edges)
        {
            return new LabeledTree<N, E>(label, edges);
        }

        public static LabeledTree<N, E> Node<N, E>(N label, params LabeledTree<N, E>.Edge[] edges)
        {
            return new LabeledTree<N, E>(label, edges);
        }

        public static LabeledTree<N, E>.Edge Edge<N, E>(E label, LabeledTree<N, E> target)
        {
            return new LabeledTree<N, E>.Edge(label, target);
        }
    }
}
