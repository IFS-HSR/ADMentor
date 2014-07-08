using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using Utils;

namespace UtilsTests
{
    [TestClass]
    public class OptionConverterTests
    {
        [TestMethod]
        public void ConvertSomeIntFromString()
        {
            AssertConversionFromString("Some(123)", Options.Some(123));
        }

        [TestMethod]
        public void ConvertNoneFromString()
        {
            AssertConversionFromString("None", Options.None<int>());
        }

        [TestMethod]
        public void ConvertSomeSomeStringFromString()
        {
            AssertConversionFromString("Some(Some(hi))", Options.Some(Options.Some("hi")));
        }

        [TestMethod]
        public void ConvertSomeIntToString()
        {
            AssertConversionToString(Options.Some(123), "Some(123)");
        }

        [TestMethod]
        public void ConvertNoneToString()
        {
            AssertConversionToString(Options.None<int>(), "None");
        }

        [TestMethod]
        public void ConvertSomeSomeStringToString()
        {
            AssertConversionToString(Options.Some(Options.Some("hi")), "Some(Some(hi))");
        }

        private void AssertConversionFromString<T>(String data, T expected) where T : class
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            var result = converter.ConvertFrom(data) as T;
            Assert.AreEqual(expected, result);
        }

        private void AssertConversionToString<T>(T value, string expected)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            var result = converter.ConvertTo(value, typeof(String));
            Assert.AreEqual(expected, result);
        }
    }
}
