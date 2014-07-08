using System;
using System.Linq;
using System.Collections.Generic;
using Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilsTests
{
    [TestClass]
    public class OptionTests
    {
        [TestMethod]
        public void TestEquals()
        {
            var none = Options.None<int>();
            var some0 = Options.Some(0);
            var some12 = Options.Some(12);
            Assert.IsTrue(none.Equals(none));
            Assert.IsTrue(some12.Equals(some12));
            Assert.IsFalse(none.Equals(some0));
            Assert.IsFalse(some0.Equals(none));

            Assert.IsTrue(some12.Equals(some0.Select(n => n + 12)));

            Assert.IsTrue(some12 == some0.Select(n => n + 12));
            Assert.IsTrue(some12 != some0);
        }

        [TestMethod]
        public void TestNullAsOptionIsNone()
        {
            String s = null;
            Assert.AreEqual(Options.None<String>(), s.AsOption());
        }

        [TestMethod]
        public void TestSomethingAsOptionIsDefined()
        {
            Assert.AreEqual(Options.Some("bird"), "bird".AsOption());
        }

        [TestMethod]
        public void TestSimpleLinqQuery()
        {
            var res = from n in Options.Some(7)
                      where n > 5
                      select n * 6;
            Assert.AreEqual(Options.Some(42), res);
        }

        [TestMethod]
        public void TestSimpleLinqQueryWithNone()
        {
            var res = from n in Options.Some(7)
                      where n < 5
                      select n + 10;
            Assert.AreEqual(Options.None<int>(), res);
        }

        [TestMethod]
        public void TestLinqWith2Sources()
        {
            var res = from n in Options.Some("hi")
                      from m in Options.Some("!")
                      select n + m;
            Assert.AreEqual(Options.Some("hi!"), res);
        }

        [TestMethod]
        public void TestLinqWithEnumerableSource()
        {
            var greetings = new List<string> { "hi", "hello" };
            var res = from g in greetings
                      from s in Options.Some("!")
                      select g + s;
            Assert.AreEqual("hi!", res.ElementAt(0));
            Assert.AreEqual("hello!", res.ElementAt(1));
        }

        [TestMethod]
        public void TestLinqWithSecondSourceAsEnumerable()
        {
            var suffixes = new List<string> { "?", "!" };
            var res = from s in Options.Some("Awesome")
                      from suffix in suffixes
                      select s + suffix;
            Assert.AreEqual("Awesome?", res.ElementAt(0));
            Assert.AreEqual("Awesome!", res.ElementAt(1));
        }
    }
}
