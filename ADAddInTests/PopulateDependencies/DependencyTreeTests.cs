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
        public void FindAlternativesForProblem()
        {
            var problemA = ElementStereotypes.Problem.Create(rootModel, "A");
            var alternativeA = ElementStereotypes.Option.Create(rootModel, "AA");
            var alternativeB = ElementStereotypes.Option.Create(rootModel, "AB");

            var cA = ConnectorStereotypes.AlternativeFor.Create(alternativeA, problemA);
            var cB = ConnectorStereotypes.AlternativeFor.Create(alternativeB, problemA);

            var expectedTree = new DependencyTree(problemA, Options.None<EA.Connector>(), new[]{
                    new DependencyTree(alternativeA, Options.Some(cA), new DependencyTree[]{}),
                    new DependencyTree(alternativeB, Options.Some(cB), new DependencyTree[]{})
                });
            AssertEqualTree(expectedTree, DependencyTree.Create(repo, problemA, 3));

            var singleNodeTree = new DependencyTree(problemA, Options.None<EA.Connector>(), new DependencyTree[] { });
            AssertEqualTree(singleNodeTree, DependencyTree.Create(repo, problemA, 1));
        }

        [TestMethod]
        public void FindIncludedProblemWithAlternatives()
        {
            var problemA = ElementStereotypes.Problem.Create(rootModel, "A");
            var problemB = ElementStereotypes.Problem.Create(rootModel, "B");
            var alternativeBA = ElementStereotypes.Option.Create(rootModel, "BA");

            var cAtoB = ConnectorStereotypes.Includes.Create(problemA, problemB);
            var cBtoBA = ConnectorStereotypes.AlternativeFor.Create(alternativeBA, problemB);

            var expextedFromA = new DependencyTree(problemA, Options.None<EA.Connector>(), new[]{
                    new DependencyTree(problemB, Options.Some(cAtoB), new[]{
                        new DependencyTree(alternativeBA, Options.Some(cBtoBA), new DependencyTree[]{})
                    })
                });
            AssertEqualTree(expextedFromA, DependencyTree.Create(repo, problemA, 3));

            var expectedFromB = new DependencyTree(problemB, Options.None<EA.Connector>(), new[]{
                        new DependencyTree(alternativeBA, Options.Some(cBtoBA), new DependencyTree[]{})
                    });
            AssertEqualTree(expectedFromB, DependencyTree.Create(repo, problemB, 3));
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

        private static void AssertEqualTree(DependencyTree expected, DependencyTree actual)
        {
            Assert.AreEqual(expected.Element.ElementGUID, actual.Element.ElementGUID);
            AssertEqualOption(expected.IcomingConnector, actual.IcomingConnector, c => c.ConnectorGUID);
            Assert.AreEqual(expected.Children.Count(), actual.Children.Count());

            expected.Children.Zip(actual.Children, (e, a) =>
            {
                AssertEqualTree(e, a);
                return Unit.Instance;
            });
        }

        private static void AssertEqualOption<T, R>(Option<T> expected, Option<T> actual, Func<T, R> extract)
        {
            Assert.AreEqual(expected.IsDefined, actual.IsDefined);
            expected.Do(e =>
            {
                actual.Do(a =>
                {
                    Assert.AreEqual(extract(e), extract(a));
                });
            });
        }
    }
}
