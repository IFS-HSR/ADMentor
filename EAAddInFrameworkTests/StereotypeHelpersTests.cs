using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EAAddInFramework;
using EAAddInFramework.MDGBuilder;

namespace EAAddInFrameworkTests
{
    [TestClass]
    public class StereotypeHelpersTests
    {
        ConnectorStereotype MyAggregation = new ConnectorStereotype(
            name: "myAggregation",
            displayName: "Is Aggregated Of",
            type: ConnectorType.Association,
            compositionKind: CompositionKind.AggregateAtSource);

        ConnectorStereotype MyDependency = new ConnectorStereotype(
            name: "myDependency",
            displayName: "Depends On",
            type: ConnectorType.Dependency,
            direction: Direction.SourceToDestination);

        [TestMethod]
        public void UseCompositionKindWhenCreatingConnectors()
        {
            var rut = new RepositoryUnderTest();

            var a = ElementType.Class.DefaultStereotype.Create(rut.TestPackage, "A");
            var b = ElementType.Class.DefaultStereotype.Create(rut.TestPackage, "B");

            var c = MyAggregation.Create(a, b);

            Assert.AreEqual((int)CompositionKind.CompositionType.Shared, c.ClientEnd.Aggregation);
            Assert.AreEqual((int)CompositionKind.CompositionType.None, c.SupplierEnd.Aggregation);
        }

        [TestMethod]
        public void UseDirectionWhenCreatingConnectors()
        {
            var rut = new RepositoryUnderTest();

            var a = ElementType.Class.DefaultStereotype.Create(rut.TestPackage, "A");
            var b = ElementType.Class.DefaultStereotype.Create(rut.TestPackage, "B");

            var c = MyDependency.Create(a, b);

            Assert.IsTrue(c.SupplierEnd.IsNavigable);
            Assert.AreEqual(Direction.Navigateability.Navigable.Name, c.SupplierEnd.Navigable);
            Assert.IsFalse(c.ClientEnd.IsNavigable);
            Assert.AreEqual(Direction.Navigateability.Unspecified.Name, c.ClientEnd.Navigable);
        }
    }
}
