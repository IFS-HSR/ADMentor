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
        private static String path = AppDomain.CurrentDomain.BaseDirectory + "\\testModel.eap";
        private static EA.Repository repo;
        private static EA.Package rootModel;

        [TestMethod]
        public void DontFollowCycles()
        {
            var a = ElementStereotypes.Problem.Create(rootModel, "A");
            var b = ElementStereotypes.Problem.Create(rootModel, "B");
            var c = ElementStereotypes.Problem.Create(rootModel, "C");

            var cAtoB = ConnectorStereotypes.Includes.Create(a, b);
            var cBtoC = ConnectorStereotypes.Includes.Create(b, c);
            var cCtoA = ConnectorStereotypes.Includes.Create(c, a);

            //var expectedTree = new DependencyTree.Node(a, new[]{
            //    new DependencyTree.Edge(cAtoB, new DependencyTree.Node(b, new []{
            //        new DependencyTree.Edge(cBtoC, new DependencyTree.Node(c, new DependencyTree.Edge[]{}))
            //    }))
            //});
            var expectedTree = LabeledTree.Create(a,
                Tuple.Create(cAtoB, LabeledTree.Create(b,
                    Tuple.Create(cBtoC, LabeledTree.Create<EA.Element, EA.Connector>(c)))));
            AssertEqualTree(expectedTree, DependencyTree.Create(repo, a));
        }

        [TestMethod]
        public void FindAlternativesForProblem()
        {
            var problemA = ElementStereotypes.Problem.Create(rootModel, "A");
            var alternativeA = ElementStereotypes.Option.Create(rootModel, "AA");
            var alternativeB = ElementStereotypes.Option.Create(rootModel, "AB");

            var cA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeA);
            var cB = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeB);

            var expectedTree = LabeledTree.Create(problemA,
                Tuple.Create(cA, LabeledTree.Create<EA.Element, EA.Connector>(alternativeA)),
                Tuple.Create(cB, LabeledTree.Create<EA.Element, EA.Connector>(alternativeB)));
            AssertEqualTree(expectedTree, DependencyTree.Create(repo, problemA));

            var singleNodeTree = LabeledTree.Create<EA.Element, EA.Connector>(problemA);
            AssertEqualTree(singleNodeTree, DependencyTree.Create(repo, problemA, levels: 0));
        }

        [TestMethod]
        public void FindIncludedProblemWithAlternatives()
        {
            var problemA = ElementStereotypes.Problem.Create(rootModel, "A");
            var problemB = ElementStereotypes.Problem.Create(rootModel, "B");
            var alternativeBA = ElementStereotypes.Option.Create(rootModel, "BA");

            var cAtoB = ConnectorStereotypes.Includes.Create(problemA, problemB);
            var cBtoBA = ConnectorStereotypes.HasAlternative.Create(problemB, alternativeBA);

            var expectedFromA = LabeledTree.Create(problemA,
                Tuple.Create(cAtoB, LabeledTree.Create(problemB,
                    Tuple.Create(cBtoBA, LabeledTree.Create<EA.Element, EA.Connector>(alternativeBA)))));
            AssertEqualTree(expectedFromA, DependencyTree.Create(repo, problemA));

            var expectedFromB = LabeledTree.Create(problemB,
                Tuple.Create(cBtoBA, LabeledTree.Create<EA.Element, EA.Connector>(alternativeBA)));
            AssertEqualTree(expectedFromB, DependencyTree.Create(repo, problemB));
        }

        [TestMethod]
        public void FindBoundAlternative()
        {
            var problemA = ElementStereotypes.Problem.Create(rootModel, "A");
            var alternativeAA = ElementStereotypes.Option.Create(rootModel, "AA");
            var problemB = ElementStereotypes.Problem.Create(rootModel, "B");
            var alternativeBA = ElementStereotypes.Option.Create(rootModel, "BA");

            var cAtoAA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAA);
            var cBtoBA = ConnectorStereotypes.HasAlternative.Create(problemB, alternativeBA);
            var cAAtoBA = ConnectorStereotypes.BoundTo.Create(alternativeAA, alternativeBA);

            //var expectedFromA = new DependencyTree.Node(problemA, new[]{
            //    new DependencyTree.Edge(cAtoAA, new DependencyTree.Node(alternativeAA, new DependencyTree.Edge[]{
            //        new DependencyTree.Edge(cAAtoBA, new DependencyTree.Node(alternativeBA, new DependencyTree.Edge[]{}))
            //    }))
            //});
            var expectedFromA = LabeledTree.Create(problemA,
                Tuple.Create(cAtoAA, LabeledTree.Create(alternativeAA,
                    Tuple.Create(cAAtoBA, LabeledTree.Create<EA.Element, EA.Connector>(alternativeBA)))));
            AssertEqualTree(expectedFromA, DependencyTree.Create(repo, problemA));

            //var expectedFromB = new DependencyTree.Node(problemB, new[]{
            //    new DependencyTree.Edge(cBtoBA, new DependencyTree.Node(alternativeBA, new DependencyTree.Edge[]{
            //        new DependencyTree.Edge(cAAtoBA, new DependencyTree.Node(alternativeAA, new DependencyTree.Edge[]{}))
            //    }))
            //});
            var expectedFromB = LabeledTree.Create(problemB,
                Tuple.Create(cBtoBA, LabeledTree.Create(alternativeBA,
                    Tuple.Create(cAAtoBA, LabeledTree.Create<EA.Element, EA.Connector>(alternativeAA)))));
            AssertEqualTree(expectedFromB, DependencyTree.Create(repo, problemB));
        }

        [TestMethod]
        public void EnumerateDependencies()
        {
            var problemA = ElementStereotypes.Problem.Create(rootModel, "A");
            var alternativeAA = ElementStereotypes.Option.Create(rootModel, "AA");
            var alternativeBA = ElementStereotypes.Option.Create(rootModel, "BA");

            var cAtoAA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAA);
            var cAAtoBA = ConnectorStereotypes.BoundTo.Create(alternativeAA, alternativeBA);

            var expectedOrder = "A,AA,BA";
            var actualOrder = DependencyTree.Create(repo, problemA).NodeLabels.Select(e => e.Name).Join(",");

            Assert.AreEqual(expectedOrder, actualOrder);
        }

        [ClassInitialize]
        public static void SetupRepository(TestContext ctx)
        {
            repo = new EA.Repository();
            repo.CreateModel(EA.CreateModelType.cmEAPFromBase, path, 0);
            repo.OpenFile(path);
        }

        [TestInitialize]
        public void CreateRootModel()
        {
            rootModel = repo.Models.AddNew("RootModel", "") as EA.Package;
            if (!rootModel.Update())
            {
                throw new ApplicationException(rootModel.GetLastError());
            }
        }

        [TestCleanup]
        public void RemoveModels()
        {
            Enumerable.Range(0, repo.Models.Count).ForEach(i =>
            {
                repo.Models.DeleteAt((short)i, true);
            });
        }

        [ClassCleanup]
        public static void TearDownRepository()
        {
            repo.Exit();
            File.Delete(path);
        }

        private static void AssertEqualTree(DependencyTree.Node expected, DependencyTree.Node actual)
        {
            Assert.AreEqual(expected.Element.ElementGUID, actual.Element.ElementGUID);
            Assert.AreEqual(expected.Children.Count(), actual.Children.Count());

            expected.Children.Zip(actual.Children).ForEach((e, a) =>
            {
                Assert.AreEqual(e.Connector.ConnectorGUID, a.Connector.ConnectorGUID);
                AssertEqualTree(e.Node, a.Node);
            });
        }

        private static void AssertEqualTree(LabeledTree<EA.Element, EA.Connector> expected, LabeledTree<EA.Element, EA.Connector> actual)
        {
            Assert.AreEqual(expected.Label.ElementGUID, actual.Label.ElementGUID);
            Assert.AreEqual(expected.Edges.Count(), actual.Edges.Count());

            expected.Edges.Zip(actual.Edges).ForEach((e, a) =>
            {
                Assert.AreEqual(e.Key.ConnectorGUID, a.Key.ConnectorGUID);
                AssertEqualTree(e.Value, a.Value);
            });
        }
    }
}
