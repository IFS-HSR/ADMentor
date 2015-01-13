using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EAAddInBase.Utils;
using System.Collections.Generic;

namespace EAAddInBase.Utils.Tests
{
    [TestClass]
    public class LabeledTreeTests
    {
        [TestMethod]
        public void SelectTreeNodes()
        {
            var tree = LabeledTree.Node(1,
                LabeledTree.Edge("a", LabeledTree.Node<int, string>(11)),
                LabeledTree.Edge("b", LabeledTree.Node<int, string>(12)));

            var nodeOrder = new List<int>();
            var edgeOrder = new List<string>();

            var expectedTree = LabeledTree.Node(1,
                LabeledTree.Edge("a", LabeledTree.Node<int, string>(12)),
                LabeledTree.Edge("b", LabeledTree.Node<int, string>(13)));

            var actualTree = tree.TransformTopDown((source, edge, target) =>
            {
                nodeOrder.Add(target);
                edgeOrder.Add(edge);
                return source + target;
            });

            AssertEqualTree(expectedTree, actualTree);

            Assert.AreEqual(2, nodeOrder.Count);
            Assert.AreEqual(2, edgeOrder.Count);
        }

        private void AssertEqualTree(LabeledTree<int, string> expectedTree, LabeledTree<int, string> actualTree)
        {
            Assert.AreEqual(expectedTree.Label, actualTree.Label);
            Assert.AreEqual(expectedTree.Edges.Count(), actualTree.Edges.Count());
            expectedTree.Edges.Zip(actualTree.Edges).ForEach((e, a) =>
            {
                Assert.AreEqual(e.Label, a.Label);
                AssertEqualTree(e.Target, a.Target);
            });
        }
    }
}
