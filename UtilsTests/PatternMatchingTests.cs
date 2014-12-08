using System;
using System.Linq;
using System.Collections.Generic;
using Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilsTests
{
    interface I { }
    class A : I { }
    class B : I { }

    [TestClass]
    public class PatternMatchingTests
    {
        [TestMethod]
        public void TestSimplePattern()
        {
            var i = new A();

            var res = i.Match<I, String>()
                .Case((A a) => "a")
                .Case((B b) => "b")
                .GetOrNone();

            Assert.AreEqual(Options.Some("a"), res);
        }

        [TestMethod]
        public void TestFailingPattern()
        {
            var i = new A();

            var res = i.Match<I, String>()
                .Case((B b) => "b")
                .GetOrNone();

            Assert.AreEqual(Options.None<String>(), res);
        }
    }
}
