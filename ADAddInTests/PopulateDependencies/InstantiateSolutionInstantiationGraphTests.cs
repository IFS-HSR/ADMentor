using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EAAddInFramework;
using AdAddIn.DataAccess;
using Utils;
using AdAddIn.ADTechnology;
using AdAddIn.PopulateDependencies;

namespace ADAddInTests.PopulateDependencies
{
    [TestClass]
    public class InstantiateSolutionInstantiationGraphTests
    {
        //[TestMethod]
        //public void InstantiateSimpleProblemSpace()
        //{
        //    var rut = new RepositoryUnderTest();
        //    var adRepo = new ElementRepository(new Atom<EA.Repository>(rut.Repo));

        //    var problemA = ProblemSpace.Problem.Create(rut.TestPackage, "A");
        //    var alternativeAA = ProblemSpace.Option.Create(rut.TestPackage, "AA");
        //    var alternativeAB = ProblemSpace.Option.Create(rut.TestPackage, "AB");

        //    var cAtoAA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAA);
        //    var cAtoAB = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAB);

        //    var occurrence1 = ProblemSpace.Problem.Instanciate(problemA, rut.TestPackage).Value;

        //    var problemSpace = SolutionInstantiationGraph.Create(adRepo, occurrence1).Value;

        //    var markedProblemSpace = problemSpace.WithSelection(new[] { new ElementInstantiation(alternativeAA, selected: true) });

        //    var result = markedProblemSpace.InstantiateSelectedItems(rut.TestPackage);

        //    result.Graph.NodeLabels.ForEach(s =>
        //    {
        //        if (s.Element.Name == "AA")
        //        {
        //            Assert.IsTrue(s.Instance.IsDefined);
        //            var instance = s.Instance.Value;
        //            Assert.IsTrue(instance.Is(Solution.OptionOccurrence));
        //            Assert.AreEqual(alternativeAA.ElementID, instance.ClassifierID);
        //        }
        //        else if (s.Element.Name == "AB")
        //        {
        //            Assert.IsFalse(s.Instance.IsDefined);
        //        }
        //    });
        //}
    }
}
