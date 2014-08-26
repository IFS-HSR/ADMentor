using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils;

namespace UtilsTests
{
    [TestClass]
    public class DirectedLabeledGraphToTreeTests
    {
        [TestMethod]
        public void TransformSimpleGraphToTree()
        {
            var graph = DirectedLabeledGraph.Create(
                    Tuple.Create(1, "a", 2),
                    Tuple.Create(1, "b", 3),
                    Tuple.Create(2, "c", 3)
                );

            var expected = LabeledTree.Node(1,
                LabeledTree.Edge("a", LabeledTree.Node(2,
                    LabeledTree.Edge("c", LabeledTree.Node<int, string>(3)))),
                LabeledTree.Edge("b", LabeledTree.Node<int, string>(3)));

            AssertEqual(expected, graph.ToTree());
        }

        [TestMethod]
        public void OnceFilter()
        {
            var graph = DirectedLabeledGraph.Create(
                    Tuple.Create("1", "a", "2"),
                    Tuple.Create("2", "a", "1")
                );

            var expected = LabeledTree.Node("1",
                LabeledTree.Edge("a", LabeledTree.Node<string, string>("2")));

            var filter = DirectedLabeledGraph.TraverseEdgeOnlyOnce(EqualityComparer<string>.Default);

            AssertEqual(expected, graph.ToTree(filter));
        }

        private void AssertEqual<N,E>(LabeledTree<N, E> expectedTree, LabeledTree<N, E> actualTree)
        {
            Assert.AreEqual(expectedTree.Label, actualTree.Label);
            Assert.AreEqual(expectedTree.Edges.Count(), actualTree.Edges.Count());
            expectedTree.Edges.Zip(actualTree.Edges).ForEach((e, a) =>
            {
                Assert.AreEqual(e.Label, a.Label);
                AssertEqual(e.Target, a.Target);
            });
        }
    }
}
