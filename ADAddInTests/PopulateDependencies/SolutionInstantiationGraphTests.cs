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
        //[TestMethod]
        //public void CreateSimpleSolutionInstantiationGraph()
        //{
        //    var rut = new RepositoryUnderTest();
        //    var adRepo = new ElementRepository(new Atom<EA.Repository>(rut.Repo));

        //    var problemA = ProblemSpace.Problem.Create(rut.TestPackage, "A");
        //    var alternativeAA = ProblemSpace.Option.Create(rut.TestPackage, "AA");
        //    var problemB = ProblemSpace.Problem.Create(rut.TestPackage, "B");
        //    var alternativeBA = ProblemSpace.Option.Create(rut.TestPackage, "BA");

        //    var cAtoAA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAA);
        //    var cBtoBA = ConnectorStereotypes.HasAlternative.Create(problemB, alternativeBA);
        //    var cAAtoBA = ConnectorStereotypes.BoundTo.Create(alternativeAA, alternativeBA);

        //    var occurrence1 = ProblemSpace.Problem.Instanciate(problemA, rut.TestPackage).Value;
        //    var decision11 = ProblemSpace.Option.Instanciate(alternativeAA, rut.TestPackage).Value;
        //    var occurrence2 = ProblemSpace.Problem.Instanciate(problemB, rut.TestPackage).Value;

        //    var c1to11 = ConnectorStereotypes.HasAlternative.Create(occurrence1, decision11);

        //    var expectedGraph = SolutionInstantiationGraph.Create(
        //            Tuple.Create(new ElementInstantiation(problemA, occurrence1), cAtoAA, new ElementInstantiation(alternativeAA, decision11)),
        //            Tuple.Create(new ElementInstantiation(alternativeAA, decision11), cAtoAA, new ElementInstantiation(problemA, occurrence1)),
        //            Tuple.Create(new ElementInstantiation(alternativeAA, decision11), cAAtoBA, new ElementInstantiation(alternativeBA)),
        //            Tuple.Create(new ElementInstantiation(alternativeBA), cAAtoBA, new ElementInstantiation(alternativeAA, decision11)),
        //            Tuple.Create(new ElementInstantiation(problemB), cBtoBA, new ElementInstantiation(alternativeBA)),
        //            Tuple.Create(new ElementInstantiation(alternativeBA), cBtoBA, new ElementInstantiation(problemB))
        //        );

        //    var actualGraph = SolutionInstantiationGraph.Create(adRepo, occurrence1).Value;

        //    AssertEqual(expectedGraph, actualGraph);
        //}

        //[TestMethod]
        //public void CreateSolutionInstatiationGraphFromDecision()
        //{
        //    var rut = new RepositoryUnderTest();
        //    var adRepo = new ElementRepository(new Atom<EA.Repository>(rut.Repo));

        //    var problemA = ProblemSpace.Problem.Create(rut.TestPackage, "A");
        //    var alternativeAA = ProblemSpace.Option.Create(rut.TestPackage, "AA");
        //    var alternativeAB = ProblemSpace.Option.Create(rut.TestPackage, "AB");

        //    var cAtoAA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAA);
        //    var cAtoAB = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAB);

        //    var decision11 = ProblemSpace.Problem.Instanciate(alternativeAA, rut.TestPackage).Value;

        //    var expected = SolutionInstantiationGraph.Create(
        //            Tuple.Create(new ElementInstantiation(alternativeAA, decision11), cAtoAA, new ElementInstantiation(problemA)),
        //            Tuple.Create(new ElementInstantiation(problemA), cAtoAA, new ElementInstantiation(alternativeAA, decision11)),
        //            Tuple.Create(new ElementInstantiation(problemA), cAtoAB, new ElementInstantiation(alternativeAB)),
        //            Tuple.Create(new ElementInstantiation(alternativeAB), cAtoAB, new ElementInstantiation(problemA))
        //        );

        //    var actual = SolutionInstantiationGraph.Create(adRepo, decision11).Value;

        //    AssertEqual(expected, actual);
        //}

        //[TestMethod]
        //public void CreateGraphWithExistingInstances()
        //{
        //    var rut = new RepositoryUnderTest();
        //    var adRepo = new ElementRepository(new Atom<EA.Repository>(rut.Repo));

        //    var problemA = ProblemSpace.Problem.Create(rut.TestPackage, "A");
        //    var alternativeAA = ProblemSpace.Option.Create(rut.TestPackage, "AA");
        //    var alternativeAB = ProblemSpace.Problem.Create(rut.TestPackage, "AB");

        //    var cAtoAA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAA);
        //    var cAtoAB = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAB);

        //    var problemOccurrence1 = adRepo.Instanciate(problemA, rut.TestPackage).Value;
        //    var decision11 = adRepo.Instanciate(alternativeAA, rut.TestPackage).Value;
        //    var decision12 = adRepo.Instanciate(alternativeAB, rut.TestPackage).Value;

        //    var c1to11 = ConnectorStereotypes.HasAlternative.Create(problemOccurrence1, decision11);
        //    var c1to12 = ConnectorStereotypes.HasAlternative.Create(problemOccurrence1, decision12);

        //    var expectedGraph = SolutionInstantiationGraph.Create(
        //            Tuple.Create(new ElementInstantiation(problemA, problemOccurrence1), cAtoAA, new ElementInstantiation(alternativeAA, decision11)),
        //            Tuple.Create(new ElementInstantiation(alternativeAA, decision11), cAtoAA, new ElementInstantiation(problemA, problemOccurrence1)),
        //            Tuple.Create(new ElementInstantiation(problemA, problemOccurrence1), cAtoAB, new ElementInstantiation(alternativeAB, decision12)),
        //            Tuple.Create(new ElementInstantiation(alternativeAB, decision12), cAtoAB, new ElementInstantiation(problemA, problemOccurrence1))
        //        );

        //    var actualGraph = SolutionInstantiationGraph.Create(adRepo, problemOccurrence1).Value;

        //    AssertEqual(expectedGraph, actualGraph);
        //}

        //private void AssertEqual(SolutionInstantiationGraph expectedGraph, SolutionInstantiationGraph actualGraph)
        //{
        //    var cc = new DependencyGraph.ConnectorComparer();
        //    Assert.AreEqual(expectedGraph.Graph.Edges.Count(), actualGraph.Graph.Edges.Count());

        //    expectedGraph.Graph.Edges.ForEach(expectedEdge =>
        //    {
        //        Assert.IsTrue(actualGraph.Graph.Edges.Any(actualEdge =>
        //            expectedEdge.Item1.Equals(actualEdge.Item1)
        //            && cc.Equals(expectedEdge.Item2, actualEdge.Item2)
        //            && expectedEdge.Item3.Equals(actualEdge.Item3)));
        //    });
        //}
    }
}
