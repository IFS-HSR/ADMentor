﻿using EAAddInFramework;
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdAddIn.ADTechnology;
using Utils;
using AdAddIn.PopulateDependencies;
using AdAddIn.DataAccess;

namespace ADAddInTests.PopulateDependencies
{
    [TestClass]
    public class InstatiateSolutionInstantiationTreeTest
    {
        [TestMethod]
        public void InstantiateSimpleProblemSpace()
        {
            var rut = new RepositoryUnderTest();
            var adRepo = new ElementRepository(new Atom<EA.Repository>(rut.Repo));

            var problemA = ElementStereotypes.Problem.Create(rut.TestPackage, "A");
            var alternativeAA = ElementStereotypes.Option.Create(rut.TestPackage, "AA");
            var alternativeAB = ElementStereotypes.Option.Create(rut.TestPackage, "AB");

            var cAtoAA = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAA);
            var cAtoAB = ConnectorStereotypes.HasAlternative.Create(problemA, alternativeAB);

            var occurrence1 = ElementStereotypes.Problem.Instanciate(problemA, rut.TestPackage).Value;

            var problemSpace = SolutionInstantiationTree.Create(adRepo, occurrence1).Value;

            var markedProblemSpace = problemSpace.TransformTopDown((parent, connector, child) =>
            {
                return child.Element.Name == "AA" ? child.Copy(selected: true) : child;
            });

            var result = SolutionInstantiationTree.InstantiateSelectedItems(adRepo, rut.TestPackage, markedProblemSpace);

            result.NodeLabels.ForEach(s =>
            {
                if (s.Element.Name == "AA")
                {
                    Assert.IsTrue(s.Instance.IsDefined);
                    var instance = s.Instance.Value;
                    Assert.IsTrue(instance.Is(ElementStereotypes.OptionOccurrence));
                    Assert.AreEqual(alternativeAA.ElementID, instance.ClassifierID);
                }
                else if (s.Element.Name == "AB")
                {
                    Assert.IsFalse(s.Instance.IsDefined);
                }
            });
        }
    }
}
