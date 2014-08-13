using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ADAddInTests;
using AdAddIn.ADTechnology;
using EAAddInFramework.DataAccess;
using AdAddIn.PopulateDependencies;
using Utils;
using System.IO;

namespace ADAddInTest.PopulateDependencies
{
    [TestClass]
    public class DependencyTreeTests
    {
        [TestMethod]
        public void DontFollowCycles()
        {
            var rut = new RepositoryUnderTest();

            var a = ElementStereotypes.Problem.Create(rut.TestPackage, "A");
            var b = ElementStereotypes.Problem.Create(rut.TestPackage, "B");
            var c = ElementStereotypes.Problem.Create(rut.TestPackage, "C");

            var cAtoB = ConnectorStereotypes.Includes.Create(a, b);
            var cBtoC = ConnectorStereotypes.Includes.Create(b, c);
            var cCtoA = ConnectorStereotypes.Includes.Create(c, a);

            var expectedTree = LabeledTree.Node(a,
                LabeledTree.Edge(cAtoB, LabeledTree.Node(b,
                    LabeledTree.Edge(cBtoC, LabeledTree.Node<EA.Element, EA.Connector>(c)))));
            AssertEqualTree(expectedTree, DependencyTree.Create(rut.Repo, a, levels: 100));
        }

        [TestMethod]
        public void FindAlternativesForProblem()
        {
            var rut = new RepositoryUnderTest();

            var problemA = ElementStereotypes.Problem.Create(rut.TestPackage, "A");
            var alternativeA = ElementStereotypes.Option.Create(rut.TestPackage, "AA");
            var alternativeB = ElementStereotypes.Option.Create(rut.TestPackage, "AB");

            var cA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeA);
            var cB = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeB);

            var expectedTree = LabeledTree.Node(problemA,
                LabeledTree.Edge(cA, LabeledTree.Node<EA.Element, EA.Connector>(alternativeA)),
                LabeledTree.Edge(cB, LabeledTree.Node<EA.Element, EA.Connector>(alternativeB)));
            AssertEqualTree(expectedTree, DependencyTree.Create(rut.Repo, problemA));

            var singleNodeTree = LabeledTree.Node<EA.Element, EA.Connector>(problemA);
            AssertEqualTree(singleNodeTree, DependencyTree.Create(rut.Repo, problemA, levels: 0));
        }

        [TestMethod]
        public void FindIncludedProblemWithAlternatives()
        {
            var rut = new RepositoryUnderTest();

            var problemA = ElementStereotypes.Problem.Create(rut.TestPackage, "A");
            var problemB = ElementStereotypes.Problem.Create(rut.TestPackage, "B");
            var alternativeBA = ElementStereotypes.Option.Create(rut.TestPackage, "BA");

            var cAtoB = ConnectorStereotypes.Includes.Create(problemA, problemB);
            var cBtoBA = ConnectorStereotypes.HasAlternative.Create(problemB, alternativeBA);

            var expectedFromA = LabeledTree.Node(problemA,
                LabeledTree.Edge(cAtoB, LabeledTree.Node(problemB,
                    LabeledTree.Edge(cBtoBA, LabeledTree.Node<EA.Element, EA.Connector>(alternativeBA)))));
            AssertEqualTree(expectedFromA, DependencyTree.Create(rut.Repo, problemA));

            var expectedFromB = LabeledTree.Node(problemB,
                LabeledTree.Edge(cBtoBA, LabeledTree.Node<EA.Element, EA.Connector>(alternativeBA)));
            AssertEqualTree(expectedFromB, DependencyTree.Create(rut.Repo, problemB));
        }

        [TestMethod]
        public void FindBoundAlternative()
        {
            var rut = new RepositoryUnderTest();

            var problemA = ElementStereotypes.Problem.Create(rut.TestPackage, "A");
            var alternativeAA = ElementStereotypes.Option.Create(rut.TestPackage, "AA");
            var problemB = ElementStereotypes.Problem.Create(rut.TestPackage, "B");
            var alternativeBA = ElementStereotypes.Option.Create(rut.TestPackage, "BA");

            var cAtoAA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAA);
            var cBtoBA = ConnectorStereotypes.HasAlternative.Create(problemB, alternativeBA);
            var cAAtoBA = ConnectorStereotypes.BoundTo.Create(alternativeAA, alternativeBA);

            var expectedFromA = LabeledTree.Node(problemA,
                LabeledTree.Edge(cAtoAA, LabeledTree.Node(alternativeAA,
                    LabeledTree.Edge(cAAtoBA, LabeledTree.Node<EA.Element, EA.Connector>(alternativeBA)))));
            AssertEqualTree(expectedFromA, DependencyTree.Create(rut.Repo, problemA));

            var expectedFromB = LabeledTree.Node(problemB,
                LabeledTree.Edge(cBtoBA, LabeledTree.Node(alternativeBA,
                    LabeledTree.Edge(cAAtoBA, LabeledTree.Node<EA.Element, EA.Connector>(alternativeAA)))));
            AssertEqualTree(expectedFromB, DependencyTree.Create(rut.Repo, problemB));
        }

        [TestMethod]
        public void EnumerateDependencies()
        {
            var rut = new RepositoryUnderTest();

            var problemA = ElementStereotypes.Problem.Create(rut.TestPackage, "A");
            var alternativeAA = ElementStereotypes.Option.Create(rut.TestPackage, "AA");
            var alternativeBA = ElementStereotypes.Option.Create(rut.TestPackage, "BA");

            var cAtoAA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAA);
            var cAAtoBA = ConnectorStereotypes.BoundTo.Create(alternativeAA, alternativeBA);

            var expectedOrder = "A,AA,BA";
            var actualOrder = DependencyTree.Create(rut.Repo, problemA).NodeLabels.Select(e => e.Name).Join(",");

            Assert.AreEqual(expectedOrder, actualOrder);
        }

        private static void AssertEqualTree(LabeledTree<EA.Element, EA.Connector> expected, LabeledTree<EA.Element, EA.Connector> actual)
        {
            Assert.AreEqual(expected.Label.ElementGUID, actual.Label.ElementGUID);
            Assert.AreEqual(expected.Edges.Count(), actual.Edges.Count());

            expected.Edges.Zip(actual.Edges).ForEach((e, a) =>
            {
                Assert.AreEqual(e.Label.ConnectorGUID, a.Label.ConnectorGUID);
                AssertEqualTree(e.Target, a.Target);
            });
        }
    }
}
