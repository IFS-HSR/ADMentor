using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdAddIn.ADTechnology;
using EAAddInFramework.DataAccess;
using Utils;
using AdAddIn.PopulateDependencies;

namespace ADAddInTests.PopulateDependencies
{
    [TestClass]
    public class SolutionInstantiationTreeTests
    {
        [TestMethod]
        public void CreateSimpleSolutionInstantiationTree()
        {
            var rut = new RepositoryUnderTest();

            var problemA = ElementStereotypes.Problem.Create(rut.TestPackage, "A");
            var alternativeAA = ElementStereotypes.Option.Create(rut.TestPackage, "AA");
            var problemB = ElementStereotypes.Problem.Create(rut.TestPackage, "B");
            var alternativeBA = ElementStereotypes.Option.Create(rut.TestPackage, "BA");

            var cAtoAA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAA);
            var cBtoBA = ConnectorStereotypes.HasAlternative.Create(problemB, alternativeBA);
            var cAAtoBA = ConnectorStereotypes.BoundTo.Create(alternativeAA, alternativeBA);

            var occurrence1 = ElementStereotypes.Problem.Instanciate(problemA, rut.TestPackage).Value;
            var decision11 = ElementStereotypes.Option.Instanciate(alternativeAA, rut.TestPackage).Value;
            var occurrence2 = ElementStereotypes.Problem.Instanciate(problemB, rut.TestPackage).Value;

            var c1to11 = ConnectorStereotypes.HasAlternative.Create(occurrence1, decision11);

            var expectedTree = LabeledTree.Node(new SolutionInstantiation(problemA, occurrence1.AsOption(), false),
                LabeledTree.Edge(cAtoAA, LabeledTree.Node(new SolutionInstantiation(alternativeAA, decision11.AsOption(), false),
                LabeledTree.Edge(cAAtoBA, LabeledTree.Node<SolutionInstantiation, EA.Connector>(new SolutionInstantiation(alternativeBA, Options.None<EA.Element>(), false))))));

            var actualTree = SolutionInstantiationTree.Create(rut.Repo, occurrence1).Value;

            AssertEqualTree(expectedTree, actualTree);
        }

        private void AssertEqualTree(LabeledTree<SolutionInstantiation, EA.Connector> expectedTree, LabeledTree<SolutionInstantiation, EA.Connector> actualTree)
        {
            Assert.IsTrue(expectedTree.Label.Equals(actualTree.Label));
            Assert.AreEqual(expectedTree.Edges.Count(), actualTree.Edges.Count());
            expectedTree.Edges.Zip(actualTree.Edges).ForEach((e, a) =>
            {
                Assert.AreEqual(e.Label.Stereotype, a.Label.Stereotype);
                AssertEqualTree(e.Target, a.Target);
            });
        }
    }
}
