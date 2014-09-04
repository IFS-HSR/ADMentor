using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils;

namespace UtilsTests
{
    [TestClass]
    public class FilterTests
    {
        [TestMethod]
        public void EmptyOrFilterAcceptsNothing()
        {
            var filter = Filter.Or("", new IFilter<int>[] { });

            Assert.IsFalse(filter.Accept(0));
        }

        [TestMethod]
        public void EmptyAndFilterAcceptsAnything()
        {
            var filter = Filter.And("", new IFilter<int>[] { });

            Assert.IsTrue(filter.Accept(0));
        }

        [TestMethod]
        public void OrFilterMustMatchAtLeastOneSubfilter()
        {
            var filter = Filter.Or("", new[]{
                Filter.Create<int>("", n => n > 0),
                Filter.Create<int>("", n => n > 5)
            });

            Assert.IsFalse(filter.Accept(0));
            Assert.IsTrue(filter.Accept(1));
            Assert.IsTrue(filter.Accept(10));
        }

        [TestMethod]
        public void AndFilterMustMatchAllSubfilters()
        {
            var filter = Filter.And("", new[]{
                Filter.Create<int>("", n => n > 0),
                Filter.Create<int>("", n => n > 5)
            });

            Assert.IsFalse(filter.Accept(0));
            Assert.IsFalse(filter.Accept(1));
            Assert.IsTrue(filter.Accept(10));
        }

        [TestMethod]
        public void GuardedAndFilterMustMatchGuardAndAllSubfilters()
        {
            var filter = Filter.And("", n => n > 0, new[]{
                Filter.Create<int>("", n => n > 5)
            });

            Assert.IsFalse(filter.Accept(0));
            Assert.IsFalse(filter.Accept(1));
            Assert.IsTrue(filter.Accept(10));
        }

        [TestMethod]
        public void CopyOrFilter()
        {
            var filter = Filter.Or("", new[]{
                Filter.Create<int>("", n => n > 0)
            });

            var copied = filter.Copy(filters: new[]{
                Filter.Create<int>("", n => n == 0)
            });

            Assert.IsTrue(copied.Accept(0));
            Assert.IsFalse(copied.Accept(1));
        }
    }
}
