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

        [TestMethod]
        public void CountLevels()
        {
            var tree = LabeledTree.Node(1,
                LabeledTree.Edge("a", LabeledTree.Node<int, string>(11)),
                LabeledTree.Edge("b", LabeledTree.Node<int, string>(12,
                    LabeledTree.Edge("c", LabeledTree.Node<int, string>(121)))));

            Assert.AreEqual(0, tree.Level(1));
            Assert.AreEqual(1, tree.Level(11));
            Assert.AreEqual(1, tree.Level(12));
            Assert.AreEqual(2, tree.Level(121));
            Assert.AreEqual(int.MaxValue, tree.Level(1211));
        }

        [TestMethod]
        public void CountLeafs()
        {
            var tree = LabeledTree.Node(1,
                LabeledTree.Edge("a", LabeledTree.Node<int, string>(11,
                    LabeledTree.Edge("c", LabeledTree.Node<int, string>(111)),
                    LabeledTree.Edge("c", LabeledTree.Node<int, string>(112)))),
                LabeledTree.Edge("b", LabeledTree.Node<int, string>(12,
                    LabeledTree.Edge("c", LabeledTree.Node<int, string>(121)))));

            Assert.AreEqual(3, tree.Leafs());
            Assert.AreEqual(3, tree.Leafs(1));
            Assert.AreEqual(2, tree.Leafs(11));
            Assert.AreEqual(1, tree.Leafs(12));
            Assert.AreEqual(1, tree.Leafs(121));
            Assert.AreEqual(0, tree.Leafs(999));
        }

        [TestMethod]
        public void CountLeftHandLeafs()
        {
            var tree = LabeledTree.Node(1,
                LabeledTree.Edge("a", LabeledTree.Node<int, string>(11,
                    LabeledTree.Edge("c", LabeledTree.Node<int, string>(111)),
                    LabeledTree.Edge("c", LabeledTree.Node<int, string>(112)))),
                LabeledTree.Edge("b", LabeledTree.Node<int, string>(12,
                    LabeledTree.Edge("c", LabeledTree.Node<int, string>(121)))));

            Assert.AreEqual(0, tree.LeftHandLeafs(1));
            Assert.AreEqual(0, tree.LeftHandLeafs(11));
            Assert.AreEqual(0, tree.LeftHandLeafs(111));
            Assert.AreEqual(1, tree.LeftHandLeafs(112));
            Assert.AreEqual(2, tree.LeftHandLeafs(12));
            Assert.AreEqual(2, tree.LeftHandLeafs(121));
            Assert.AreEqual(3, tree.LeftHandLeafs(1211));
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
