using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdAddIn.DataAccess;
using Utils;

namespace ADAddInTests.DataAccess
{
    [TestClass]
    public class ModelFilterTest
    {
        [TestMethod]
        public void FindParentInFilter()
        {
            var child = new ModelFilter.Any();
            var parent = new ModelFilter.Or(new ModelFilter.Any(), child, new ModelFilter.Any());
            var filter = new ModelFilter.And(new ModelFilter.Any(), parent, new ModelFilter.Or());

            var res = filter.FindParent(child);

            Assert.IsTrue(res.IsDefined);
            Assert.AreEqual(parent, res.Value);
        }

        [TestMethod]
        public void FindParentInNonComposite()
        {
            var child = new ModelFilter.Any();
            var filter = new ModelFilter.Any();

            var res = filter.FindParent(child);

            Assert.IsFalse(res.IsDefined);
        }

        [TestMethod]
        public void ReplaceFilter()
        {
            var original = new ModelFilter.Any();
            var replacement = new ModelFilter.Any();

            var filter = new ModelFilter.And(original, new ModelFilter.Any());

            var res = filter.Replace(original, replacement);

            var firstChild = res.TryCast<ModelFilter.Composite>().Value.Filters.ElementAt(0);

            Assert.AreEqual(replacement, firstChild);
        }

        [TestMethod]
        public void ReplaceRoot()
        {
            var original = new ModelFilter.Any();
            var replacement = new ModelFilter.Any();

            var res = original.Replace(original, replacement);

            Assert.AreEqual(replacement, res);
        }

        [TestMethod]
        public void AddAlternativeToSingelFilter()
        {
            var filter = new ModelFilter.Any();
            var alt = new ModelFilter.Any();

            var res = filter.AddAlternative(filter, alt);

            Assert.IsInstanceOfType(res, typeof(ModelFilter.Or));

            var secondChild = res.TryCast<ModelFilter.Composite>().Value.Filters.ElementAt(1);
            Assert.AreEqual(alt, secondChild);
        }

        [TestMethod]
        public void AddAlternativeToOrFilter()
        {
            var filter = new ModelFilter.Or(new ModelFilter.Any(), new ModelFilter.Any());
            var alt = new ModelFilter.Any();

            var res = filter.AddAlternative(filter, alt);

            Assert.IsInstanceOfType(res, typeof(ModelFilter.Or));
            var children = res.TryCast<ModelFilter.Composite>().Value.Filters;
            Assert.AreEqual(3, children.Count());
            Assert.AreEqual(alt, children.ElementAt(2));
        }

        [TestMethod]
        public void AddAlternativeToOrFilterChild()
        {
            var selected = new ModelFilter.Any();
            var filter = new ModelFilter.Or(new ModelFilter.Any(), selected);
            var alt = new ModelFilter.Any();

            var res = filter.AddAlternative(selected, alt);

            Assert.IsInstanceOfType(res, typeof(ModelFilter.Or));
            var children = res.TryCast<ModelFilter.Composite>().Value.Filters;
            Assert.AreEqual(3, children.Count());
            Assert.AreEqual(alt, children.ElementAt(2));
        }

        [TestMethod]
        public void AddAlternativeToAndFilter()
        {
            var filter = new ModelFilter.And(new ModelFilter.Any(), new ModelFilter.Any());
            var alt = new ModelFilter.Any();

            var res = filter.AddAlternative(filter, alt);

            Assert.IsInstanceOfType(res, typeof(ModelFilter.Or));
            var children = res.TryCast<ModelFilter.Composite>().Value.Filters;
            Assert.AreEqual(2, children.Count());
            Assert.IsInstanceOfType(children.ElementAt(0), typeof(ModelFilter.And));
            Assert.AreEqual(alt, children.ElementAt(1));
        }

        [TestMethod]
        public void RemoveLastButOneChild()
        {
            var f = new ModelFilter.Any();
            var root = new ModelFilter.And(f, new ModelFilter.Any());

            var res = root.Remove(f);

            Assert.IsInstanceOfType(res, typeof(ModelFilter.Any));
        }

        [TestMethod]
        public void RemoveRoot()
        {
            var root = new ModelFilter.And(new ModelFilter.Any(), new ModelFilter.Any());

            var res = root.Remove(root);

            Assert.IsInstanceOfType(res, typeof(ModelFilter.Any));
        }
    }
}
