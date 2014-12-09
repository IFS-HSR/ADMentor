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

        [TestMethod]
        public void TestThrowOnFailingPattern()
        {
            var i = new A();

            var e = Intercept<NotImplementedException>(() =>
            {
                i.Match<I, String>()
                    .Case((B b) => "b")
                    .GetOrThrowNotImplemented();
            });

            Assert.IsTrue(e.Message.Contains(typeof(A).ToString()), "Expected message to contain {0} but got \"{1}\" instead", typeof(A).ToString(), e.Message);
        }

        private E Intercept<E>(Action act) where E : Exception
        {
            try
            {
                act();
            }
            catch (E e)
            {
                return e;
            }
            Assert.Fail("Expected exception of type {0} but got none instead", typeof(E));
            return null;
        }
    }
}
