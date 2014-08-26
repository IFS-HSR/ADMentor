using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EAAddInFramework;
using AdAddIn.DataAccess;
using AdAddIn.ADTechnology;
using Utils;
using AdAddIn.PopulateDependencies;
using System.Linq;
using System.Collections.Generic;

namespace ADAddInTests.PopulateDependencies
{
    [TestClass]
    public class SolutionInstantiationGraphTests
    {
        [TestMethod]
        public void CreateSimpleSolutionInstantiationGraph()
        {
            var rut = new RepositoryUnderTest();
            var adRepo = new ElementRepository(new Atom<EA.Repository>(rut.Repo));

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

            var expectedGraph = SolutionInstantiationGraph.Create(
                    Tuple.Create(new SolutionInstantiation(problemA, occurrence1), cAtoAA, new SolutionInstantiation(alternativeAA, decision11)),
                    Tuple.Create(new SolutionInstantiation(alternativeAA, decision11), cAtoAA, new SolutionInstantiation(problemA, occurrence1)),
                    Tuple.Create(new SolutionInstantiation(alternativeAA, decision11), cAAtoBA, new SolutionInstantiation(alternativeBA)),
                    Tuple.Create(new SolutionInstantiation(alternativeBA), cAAtoBA, new SolutionInstantiation(alternativeAA, decision11)),
                    Tuple.Create(new SolutionInstantiation(problemB), cBtoBA, new SolutionInstantiation(alternativeBA)),
                    Tuple.Create(new SolutionInstantiation(alternativeBA), cBtoBA, new SolutionInstantiation(problemB))
                );

            var actualGraph = SolutionInstantiationGraph.Create(adRepo, occurrence1).Value;

            AssertEqual(expectedGraph, actualGraph);
        }

        [TestMethod]
        public void CreateSolutionInstatiationGraphFromDecision()
        {
            var rut = new RepositoryUnderTest();
            var adRepo = new ElementRepository(new Atom<EA.Repository>(rut.Repo));

            var problemA = ElementStereotypes.Problem.Create(rut.TestPackage, "A");
            var alternativeAA = ElementStereotypes.Option.Create(rut.TestPackage, "AA");
            var alternativeAB = ElementStereotypes.Option.Create(rut.TestPackage, "AB");

            var cAtoAA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAA);
            var cAtoAB = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAB);

            var decision11 = ElementStereotypes.Problem.Instanciate(alternativeAA, rut.TestPackage).Value;

            var expected = SolutionInstantiationGraph.Create(
                    Tuple.Create(new SolutionInstantiation(alternativeAA, decision11), cAtoAA, new SolutionInstantiation(problemA)),
                    Tuple.Create(new SolutionInstantiation(problemA), cAtoAA, new SolutionInstantiation(alternativeAA, decision11)),
                    Tuple.Create(new SolutionInstantiation(problemA), cAtoAB, new SolutionInstantiation(alternativeAB)),
                    Tuple.Create(new SolutionInstantiation(alternativeAB), cAtoAB, new SolutionInstantiation(problemA))
                );

            var actual = SolutionInstantiationGraph.Create(adRepo, decision11).Value;

            AssertEqual(expected, actual);
        }

        [TestMethod]
        public void CreateGraphWithExistingInstances()
        {
            var rut = new RepositoryUnderTest();
            var adRepo = new ElementRepository(new Atom<EA.Repository>(rut.Repo));

            var problemA = ElementStereotypes.Problem.Create(rut.TestPackage, "A");
            var alternativeAA = ElementStereotypes.Option.Create(rut.TestPackage, "AA");
            var alternativeAB = ElementStereotypes.Problem.Create(rut.TestPackage, "AB");

            var cAtoAA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAA);
            var cAtoAB = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAB);

            var problemOccurrence1 = adRepo.Instanciate(problemA, rut.TestPackage).Value;
            var decision11 = adRepo.Instanciate(alternativeAA, rut.TestPackage).Value;
            var decision12 = adRepo.Instanciate(alternativeAB, rut.TestPackage).Value;

            var c1to11 = ConnectorStereotypes.HasAlternative.Create(problemOccurrence1, decision11);
            var c1to12 = ConnectorStereotypes.HasAlternative.Create(problemOccurrence1, decision12);

            var expectedGraph = SolutionInstantiationGraph.Create(
                    Tuple.Create(new SolutionInstantiation(problemA, problemOccurrence1), cAtoAA, new SolutionInstantiation(alternativeAA, decision11)),
                    Tuple.Create(new SolutionInstantiation(alternativeAA, decision11), cAtoAA, new SolutionInstantiation(problemA, problemOccurrence1)),
                    Tuple.Create(new SolutionInstantiation(problemA, problemOccurrence1), cAtoAB, new SolutionInstantiation(alternativeAB, decision12)),
                    Tuple.Create(new SolutionInstantiation(alternativeAB, decision12), cAtoAB, new SolutionInstantiation(problemA, problemOccurrence1))
                );

            var actualGraph = SolutionInstantiationGraph.Create(adRepo, problemOccurrence1).Value;

            AssertEqual(expectedGraph, actualGraph);
        }

        private void AssertEqual(DirectedLabeledGraph<SolutionInstantiation, EA.Connector> expectedGraph,
            DirectedLabeledGraph<SolutionInstantiation, EA.Connector> actualGraph)
        {
            var cc = new DependencyGraph.ConnectorComparer();
            Assert.AreEqual(expectedGraph.Edges.Count(), actualGraph.Edges.Count());

            expectedGraph.Edges.ForEach(expectedEdge =>
            {
                Assert.IsTrue(actualGraph.Edges.Any(actualEdge =>
                    expectedEdge.Item1.Equals(actualEdge.Item1)
                    && cc.Equals(expectedEdge.Item2, actualEdge.Item2)
                    && expectedEdge.Item3.Equals(actualEdge.Item3)));
            });
        }
    }
}
