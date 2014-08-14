using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdAddIn.ADTechnology;
using EAAddInFramework;
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

            var expectedTree = LabeledTree.Node(new SolutionInstantiation(problemA, occurrence1),
                LabeledTree.Edge(cAtoAA, LabeledTree.Node(new SolutionInstantiation(alternativeAA, decision11),
                    LabeledTree.Edge(cAAtoBA, LabeledTree.Node<SolutionInstantiation, EA.Connector>(new SolutionInstantiation(alternativeBA))))));

            var actualTree = SolutionInstantiationTree.Create(rut.Repo, occurrence1).Value;

            AssertEqualTree(expectedTree, actualTree);
        }

        [TestMethod]
        [Ignore]
        public void CreateSolutionInstatiationTreeFromDecision()
        {
            var rut = new RepositoryUnderTest();

            var problemA = ElementStereotypes.Problem.Create(rut.TestPackage, "A");
            var alternativeAA = ElementStereotypes.Option.Create(rut.TestPackage, "AA");
            var alternativeAB = ElementStereotypes.Option.Create(rut.TestPackage, "AB");

            var cAtoAA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAA);
            var cAtoAB = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAB);

            var decision11 = ElementStereotypes.Problem.Instanciate(alternativeAA, rut.TestPackage).Value;

            var expectedTree = LabeledTree.Node(new SolutionInstantiation(alternativeAA, decision11),
                LabeledTree.Edge(cAtoAA, LabeledTree.Node(new SolutionInstantiation(problemA),
                    LabeledTree.Edge(cAtoAB, LabeledTree.Node<SolutionInstantiation, EA.Connector>(new SolutionInstantiation(alternativeAB))))));

            var actualTree = SolutionInstantiationTree.Create(rut.Repo, decision11).Value;

            AssertEqualTree(expectedTree, actualTree);
        }

        [TestMethod]
        public void CreateTreeWithExistingInstances()
        {
            var rut = new RepositoryUnderTest();

            var problemA = ElementStereotypes.Problem.Create(rut.TestPackage, "A");
            var alternativeAA = ElementStereotypes.Option.Create(rut.TestPackage, "AA");
            var alternativeAB = ElementStereotypes.Problem.Create(rut.TestPackage, "AB");

            var cAtoAA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAA);
            var cAtoAB = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAB);

            var problemOccurrence1 = problemA.Instanciate(rut.TestPackage).Value;
            var decision11 = alternativeAA.Instanciate(rut.TestPackage).Value;
            var decision12 = alternativeAB.Instanciate(rut.TestPackage).Value;

            var c1to11 = ConnectorStereotypes.HasAlternative.Create(problemOccurrence1, decision11);
            var c1to12 = ConnectorStereotypes.HasAlternative.Create(problemOccurrence1, decision12);

            var expectedTree = LabeledTree.Node(new SolutionInstantiation(problemA, problemOccurrence1),
                LabeledTree.Edge(cAtoAA, LabeledTree.Node<SolutionInstantiation, EA.Connector>(new SolutionInstantiation(alternativeAA, decision11))),
                LabeledTree.Edge(cAtoAB, LabeledTree.Node<SolutionInstantiation, EA.Connector>(new SolutionInstantiation(alternativeAB, decision12))));

            var actualTree = SolutionInstantiationTree.Create(rut.Repo, problemOccurrence1).Value;

            var classifierIds = actualTree.NodeLabels.Select(n => n.Element.ElementID).Join(",");
            var instanceIds = actualTree.NodeLabels.Select(n => n.Instance.Select(i => i.ElementID).ToString()).Join(",");

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
