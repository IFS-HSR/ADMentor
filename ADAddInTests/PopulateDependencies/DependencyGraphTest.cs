using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EAAddInFramework;
using AdAddIn.DataAccess;
using Utils;
using AdAddIn.ADTechnology;
using AdAddIn.PopulateDependencies;

namespace ADAddInTests.PopulateDependencies
{
    [TestClass]
    public class DependencyGraphTest
    {
        [TestClass]
        public class DependencyTreeTests
        {

            [TestMethod]
            public void DontFollowCycles()
            {
                var rut = new RepositoryUnderTest();
                var adRepo = new ElementRepository(new Atom<EA.Repository>(rut.Repo));

                var a = ElementStereotypes.Problem.Create(rut.TestPackage, "A");
                var b = ElementStereotypes.Problem.Create(rut.TestPackage, "B");
                var c = ElementStereotypes.Problem.Create(rut.TestPackage, "C");

                var cAtoB = ConnectorStereotypes.Includes.Create(a, b);
                var cBtoC = ConnectorStereotypes.Includes.Create(b, c);
                var cCtoA = ConnectorStereotypes.Includes.Create(c, a);

                var expectedGraph = DirectedLabeledGraph.Create(
                        Tuple.Create(a, cAtoB, b),
                        Tuple.Create(b, cBtoC, c),
                        Tuple.Create(c, cCtoA, a)
                    );
                AssertEqualGraph(expectedGraph, DependencyGraph.Create(adRepo, a, DependencyGraph.TraverseOnlyADConnectors));
            }

            [TestMethod]
            public void FindAlternativesForProblem()
            {
                var rut = new RepositoryUnderTest();
                var adRepo = new ElementRepository(new Atom<EA.Repository>(rut.Repo));

                var problemA = ElementStereotypes.Problem.Create(rut.TestPackage, "A");
                var alternativeA = ElementStereotypes.Option.Create(rut.TestPackage, "AA");
                var alternativeB = ElementStereotypes.Option.Create(rut.TestPackage, "AB");

                var cA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeA);
                var cB = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeB);

                var expectedGraph = DirectedLabeledGraph.Create(
                        Tuple.Create(problemA, cA, alternativeA),
                        Tuple.Create(problemA, cB, alternativeB)
                    );
                AssertEqualGraph(expectedGraph, DependencyGraph.Create(adRepo, problemA, DependencyGraph.TraverseOnlyADConnectors));
            }

            [TestMethod]
            public void FindIncludedProblemWithAlternatives()
            {
                var rut = new RepositoryUnderTest();
                var adRepo = new ElementRepository(new Atom<EA.Repository>(rut.Repo));

                var problemA = ElementStereotypes.Problem.Create(rut.TestPackage, "A");
                var problemB = ElementStereotypes.Problem.Create(rut.TestPackage, "B");
                var alternativeBA = ElementStereotypes.Option.Create(rut.TestPackage, "BA");

                var cAtoB = ConnectorStereotypes.Includes.Create(problemA, problemB);
                var cBtoBA = ConnectorStereotypes.HasAlternative.Create(problemB, alternativeBA);

                var expectedFromA = DirectedLabeledGraph.Create(
                        Tuple.Create(problemA, cAtoB, problemB),
                        Tuple.Create(problemB, cBtoBA, alternativeBA)
                    );
                AssertEqualGraph(expectedFromA, DependencyGraph.Create(adRepo, problemA, DependencyGraph.TraverseOnlyADConnectors));

                var expectedFromB = DirectedLabeledGraph.Create(
                        Tuple.Create(problemB, cBtoBA, alternativeBA)
                    );
                AssertEqualGraph(expectedFromB, DependencyGraph.Create(adRepo, problemB, DependencyGraph.TraverseOnlyADConnectors));
            }

            [TestMethod]
            public void FindBoundAlternative()
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

                var expectedFromA = DirectedLabeledGraph.Create(
                        Tuple.Create(problemA, cAtoAA, alternativeAA),
                        Tuple.Create(alternativeAA, cAAtoBA, alternativeBA),
                        Tuple.Create(alternativeBA, cAAtoBA, alternativeAA)
                    );
                AssertEqualGraph(expectedFromA, DependencyGraph.Create(adRepo, problemA, DependencyGraph.TraverseOnlyADConnectors));

                var expectedFromB = DirectedLabeledGraph.Create(
                        Tuple.Create(problemB, cBtoBA, alternativeBA),
                        Tuple.Create(alternativeAA, cAAtoBA, alternativeBA),
                        Tuple.Create(alternativeBA, cAAtoBA, alternativeAA)
                    );
                AssertEqualGraph(expectedFromB, DependencyGraph.Create(adRepo, problemB, DependencyGraph.TraverseOnlyADConnectors));
            }

            private void AssertEqualGraph(DirectedLabeledGraph<EA.Element, EA.Connector> expectedGraph,
                DirectedLabeledGraph<EA.Element, EA.Connector> actualGraph)
            {
                Assert.AreEqual(expectedGraph.Edges.Count(), actualGraph.Edges.Count());

                expectedGraph.Edges.ForEach(expectedEdge =>
                {
                    Assert.IsTrue(actualGraph.Edges.Any(actualEdge =>
                        DependencyGraph.CompareElements(expectedEdge.Item1, actualEdge.Item1)
                        && DependencyGraph.CompareConnectors(expectedEdge.Item2, actualEdge.Item2)
                        && DependencyGraph.CompareElements(expectedEdge.Item3, actualEdge.Item3)));
                });
            }
        }
    }
}
