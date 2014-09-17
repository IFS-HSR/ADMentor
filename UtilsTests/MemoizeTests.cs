using System;
using Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace UtilsTests
{
    [TestClass]
    public class MemoizeTests
    {
        [TestMethod]
        public void MemoizedFunctionsReturnCorrectValue()
        {
            Func<int, int> f = i => i * i;

            var memoized = f.Memoize();

            var rnd = new Random(0);

            Enumerable.Repeat(Unit.Instance, 100).ForEach(_ =>
            {
                var v = rnd.Next(int.MinValue, int.MaxValue);
                Assert.AreEqual(f(v), memoized(v));
            });
        }

        [TestMethod]
        public void MemoizedCallsTheUnderlyingFunctionOnceForEachValue()
        {
            var calls = 0;
            Func<int, int> f = i =>
            {
                calls++;
                return i;
            };

            var memoized = f.Memoize();

            memoized(1);
            memoized(1);

            Assert.AreEqual(1, calls);

            memoized(2);

            Assert.AreEqual(2, calls);
        }

        [TestMethod]
        public void UseTuplifyToMemoizeMultiArgFunctions()
        {
            Func<int, int, int> f = (i, j) => i - j;

            var memoized = f.Tuplify().Memoize().Detuplify();

            Assert.AreEqual(f(1, 10), memoized(1, 10));
        }
    }
}
