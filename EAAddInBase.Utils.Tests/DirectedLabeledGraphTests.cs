using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EAAddInBase.Utils;
using System.Collections.Generic;

namespace EAAddInBase.Utils.Tests
{
    [TestClass]
    public class DirectedLabeledGraphTests
    {
        [TestMethod]
        public void AddConnectionToEmptyGraph()
        {
            var connected = new DirectedLabeledGraph<int, string>(1)
                .Connect(1, "1to2", 2);

            Assert.IsTrue(connected.NodeLabels.Any(l => l == 1));
            Assert.IsTrue(connected.NodeLabels.Any(l => l == 2));
            Assert.AreEqual(2, connected.NodeLabels.Count());
            Assert.IsTrue(connected.Edges.Any(e => e.Item1 == 1 && e.Item2 == "1to2" && e.Item3 == 2));
            Assert.IsFalse(connected.Edges.Any(e => e.Item1 == 2 && e.Item2 == "1to2" && e.Item3 == 1));
        }

        [TestMethod]
        public void AddCycle()
        {
            var cycle = new DirectedLabeledGraph<int, string>(1)
                .Connect(1, "1to2", 2)
                .Connect(2, "2to1", 1);

            Assert.IsTrue(cycle.NodeLabels.Any(l => l == 1));
            Assert.IsTrue(cycle.NodeLabels.Any(l => l == 2));
            Assert.AreEqual(2, cycle.NodeLabels.Count());
            Assert.IsTrue(cycle.Edges.Any(e => e.Item1 == 1 && e.Item2 == "1to2" && e.Item3 == 2));
            Assert.IsTrue(cycle.Edges.Any(e => e.Item1 == 2 && e.Item2 == "2to1" && e.Item3 == 1));
        }

        [TestMethod]
        public void EdgesAreUnique()
        {
            var graph = new DirectedLabeledGraph<int, char>(1)
                .Connect(1, 'a', 2)
                .Connect(1, 'a', 2);

            Assert.AreEqual(1, graph.Edges.Count());
        }

        [TestMethod]
        public void CreaeInReversedOrder()
        {
            var graph = new DirectedLabeledGraph<int, char>(1)
                .Connect(2, 'b', 3)
                .Connect(1, 'a', 2);

            Assert.AreEqual(3, graph.NodeLabels.Count());
            Assert.IsTrue(graph.Edges.Any(e => e.Item1 == 1 && e.Item2 == 'a' && e.Item3 == 2));
            Assert.IsTrue(graph.Edges.Any(e => e.Item1 == 2 && e.Item2 == 'b' && e.Item3 == 3));
        }

        [TestMethod]
        public void AddMultipleOutgoingEdges()
        {
            var graph = new DirectedLabeledGraph<int, char>(1)
                .Connect(1, 'a', 2)
                .Connect(1, 'b', 3);

            Assert.AreEqual(3, graph.NodeLabels.Count());
            Assert.IsTrue(graph.Edges.Any(e => e.Item1 == 1 && e.Item2 == 'a' && e.Item3 == 2));
            Assert.IsTrue(graph.Edges.Any(e => e.Item1 == 1 && e.Item2 == 'b' && e.Item3 == 3));
        }

        [TestMethod]
        public void TraverseSimpleGraph()
        {
            var graph = new DirectedLabeledGraph<int, char>(1)
                .Connect(1, 'a', 2)
                .Connect(2, 'b', 4)
                .Connect(1, 'c', 3)
                .Connect(3, 'd', 4);

            var edgeOrder = new List<char>();

            graph.TraverseEdgesBF((source, edge, target) =>
            {
                edgeOrder.Add(edge);
            });

            Assert.IsTrue(edgeOrder.IndexOf('a') >= 0);
            Assert.IsTrue(edgeOrder.IndexOf('c') >= 0);
            Assert.IsTrue(edgeOrder.IndexOf('b') > edgeOrder.IndexOf('a'));
            Assert.IsTrue(edgeOrder.IndexOf('d') > edgeOrder.IndexOf('c'));
        }

        [TestMethod]
        public void DontFollowCycles()
        {
            var graph = new DirectedLabeledGraph<int, char>(1)
                .Connect(1, 'a', 2)
                .Connect(2, 'b', 3);

            var edgeOrder = new List<char>();

            graph.TraverseEdgesBF((source, edge, target) =>
            {
                edgeOrder.Add(edge);
            });

            Assert.AreEqual(2, edgeOrder.Count());
            Assert.IsTrue(edgeOrder.IndexOf('a') >= 0);
            Assert.IsTrue(edgeOrder.IndexOf('b') > edgeOrder.IndexOf('a'));
        }

        [TestMethod]
        public void MapNodesInSimpleGraph()
        {
            var graph = new DirectedLabeledGraph<int, char>(1)
                .Connect(1, 'a', 2)
                .Connect(2, 'b', 3);

            var calls = 0;

            var mappedGraph = graph.MapNodeLabels((int n) => {
                calls++;
                return n.ToString();
            });

            Assert.AreEqual(3, calls);
            Assert.IsTrue(mappedGraph.Edges.Any(e => e.Item1 == "1" && e.Item2 == 'a' && e.Item3 == "2"));
            Assert.IsTrue(mappedGraph.Edges.Any(e => e.Item1 == "2" && e.Item2 == 'b' && e.Item3 == "3"));
        }

        [TestMethod]
        public void DetectLeafs()
        {
            var graph = new DirectedLabeledGraph<int, char>(1)
                .Connect(1, 'a', 2)
                .Connect(2, 'b', 3)
                .Connect(1, 'c', 4);

            Assert.IsFalse(graph.IsLeaf(1));
            Assert.IsFalse(graph.IsLeaf(2));
            Assert.IsTrue(graph.IsLeaf(3));
            Assert.IsTrue(graph.IsLeaf(4));
        }
    }
}
