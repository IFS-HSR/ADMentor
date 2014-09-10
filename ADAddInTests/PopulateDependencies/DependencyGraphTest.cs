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
        //[TestClass]
        //public class DependencyTreeTests
        //{
        //    private Func<EA.Element, EA.Connector, EA.Element, bool> EdgeFilter =
        //           DependencyGraph.TraverseOnlyTechnologyConnectors(AdAddIn.ADTechnology.Technologies.AD);

        //    [TestMethod]
        //    public void DontFollowCycles()
        //    {
        //        var rut = new RepositoryUnderTest();
        //        var adRepo = new ElementRepository(new Atom<EA.Repository>(rut.Repo));

        //        var a = ProblemSpace.Problem.Create(rut.TestPackage, "A");
        //        var b = ProblemSpace.Problem.Create(rut.TestPackage, "B");
        //        var c = ProblemSpace.Problem.Create(rut.TestPackage, "C");

        //        var cAtoB = ConnectorStereotypes.Includes.Create(a, b);
        //        var cBtoC = ConnectorStereotypes.Includes.Create(b, c);
        //        var cCtoA = ConnectorStereotypes.Includes.Create(c, a);

        //        var expectedGraph = DirectedLabeledGraph.Create(
        //                Tuple.Create(a, cAtoB, b),
        //                Tuple.Create(b, cBtoC, c),
        //                Tuple.Create(c, cCtoA, a)
        //            );
        //        AssertEqualGraph(expectedGraph, DependencyGraph.Create(adRepo, a, EdgeFilter));
        //    }

        //    [TestMethod]
        //    public void FindAlternativesForProblem()
        //    {
        //        var rut = new RepositoryUnderTest();
        //        var adRepo = new ElementRepository(new Atom<EA.Repository>(rut.Repo));

        //        var problemA = ProblemSpace.Problem.Create(rut.TestPackage, "A");
        //        var alternativeA = ProblemSpace.Option.Create(rut.TestPackage, "AA");
        //        var alternativeB = ProblemSpace.Option.Create(rut.TestPackage, "AB");

        //        var cA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeA);
        //        var cB = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeB);

        //        var expectedGraph = DirectedLabeledGraph.Create(
        //                Tuple.Create(problemA, cA, alternativeA),
        //                Tuple.Create(alternativeA, cA, problemA),
        //                Tuple.Create(problemA, cB, alternativeB),
        //                Tuple.Create(alternativeB, cB, problemA)
        //            );
        //        AssertEqualGraph(expectedGraph, DependencyGraph.Create(adRepo, problemA, EdgeFilter));
        //    }

        //    [TestMethod]
        //    public void FindIncludedProblemWithAlternatives()
        //    {
        //        var rut = new RepositoryUnderTest();
        //        var adRepo = new ElementRepository(new Atom<EA.Repository>(rut.Repo));

        //        var problemA = ProblemSpace.Problem.Create(rut.TestPackage, "A");
        //        var problemB = ProblemSpace.Problem.Create(rut.TestPackage, "B");
        //        var alternativeBA = ProblemSpace.Option.Create(rut.TestPackage, "BA");

        //        var cAtoB = ConnectorStereotypes.Includes.Create(problemA, problemB);
        //        var cBtoBA = ConnectorStereotypes.HasAlternative.Create(problemB, alternativeBA);

        //        var expectedFromA = DirectedLabeledGraph.Create(
        //                Tuple.Create(problemA, cAtoB, problemB),
        //                Tuple.Create(problemB, cBtoBA, alternativeBA),
        //                Tuple.Create(alternativeBA, cBtoBA, problemB)
        //            );
        //        AssertEqualGraph(expectedFromA, DependencyGraph.Create(adRepo, problemA, EdgeFilter));

        //        var expectedFromB = DirectedLabeledGraph.Create(
        //                Tuple.Create(problemB, cBtoBA, alternativeBA),
        //                Tuple.Create(alternativeBA, cBtoBA, problemB)
        //            );
        //        AssertEqualGraph(expectedFromB, DependencyGraph.Create(adRepo, problemB, EdgeFilter));
        //    }

        //    [TestMethod]
        //    public void FindBoundAlternative()
        //    {
        //        var rut = new RepositoryUnderTest();
        //        var adRepo = new ElementRepository(new Atom<EA.Repository>(rut.Repo));

        //        var problemA = ProblemSpace.Problem.Create(rut.TestPackage, "A");
        //        var alternativeAA = ProblemSpace.Option.Create(rut.TestPackage, "AA");
        //        var problemB = ProblemSpace.Problem.Create(rut.TestPackage, "B");
        //        var alternativeBA = ProblemSpace.Option.Create(rut.TestPackage, "BA");

        //        var cAtoAA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAA);
        //        var cBtoBA = ConnectorStereotypes.HasAlternative.Create(problemB, alternativeBA);
        //        var cAAtoBA = ConnectorStereotypes.BoundTo.Create(alternativeAA, alternativeBA);

        //        var expectedFromA = DirectedLabeledGraph.Create(
        //                Tuple.Create(problemA, cAtoAA, alternativeAA),
        //                Tuple.Create(alternativeAA, cAtoAA, problemA),
        //                Tuple.Create(problemB, cBtoBA, alternativeBA),
        //                Tuple.Create(alternativeBA, cBtoBA, problemB),
        //                Tuple.Create(alternativeAA, cAAtoBA, alternativeBA),
        //                Tuple.Create(alternativeBA, cAAtoBA, alternativeAA)
        //            );
        //        AssertEqualGraph(expectedFromA, DependencyGraph.Create(adRepo, problemA, EdgeFilter));

        //        var expectedFromB = expectedFromA;
        //        AssertEqualGraph(expectedFromB, DependencyGraph.Create(adRepo, problemB, EdgeFilter));
        //    }

        //    private void AssertEqualGraph(DirectedLabeledGraph<EA.Element, EA.Connector> expectedGraph,
        //        DirectedLabeledGraph<EA.Element, EA.Connector> actualGraph)
        //    {
        //        Assert.AreEqual(expectedGraph.Edges.Count(), actualGraph.Edges.Count());

        //        var ec = new DependencyGraph.ElementComparer();
        //        var cc = new DependencyGraph.ConnectorComparer();

        //        expectedGraph.Edges.ForEach(expectedEdge =>
        //        {
        //            Assert.IsTrue(actualGraph.Edges.Any(actualEdge =>
        //                ec.Equals(expectedEdge.Item1, actualEdge.Item1)
        //                && cc.Equals(expectedEdge.Item2, actualEdge.Item2)
        //                && ec.Equals(expectedEdge.Item3, actualEdge.Item3)));
        //        });
        //    }
        //}
    }
}
