using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.Analysis
{
    public static class Metrics
    {
        public static Metric Category(String name, params Metric[] members)
        {
            return new Category(name, members);
        }

        public static Metric Entry<T>(String key, T value)
        {
            return new Entry<T>(key, value);
        }
    }

    public interface Metric
    {
        String ToString(String prefix);
    }

    public class Category : Metric
    {
        public Category(String name, IEnumerable<Metric> members)
        {
            Name = name;
            Members = members;
        }

        public string Name { get; private set; }

        public IEnumerable<Metric> Members { get; private set; }

        public override string ToString()
        {
            return ToString("");
        }

        public string ToString(String prefix)
        {
            return String.Format("{0}{1}:\r\n{2}", prefix, Name, Members.Select(m => m.ToString(prefix + "  ")).Join("\r\n"));
        }
    }

    public class Entry<T> : Metric
    {
        public Entry(String key, T value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; private set; }

        public T Value { get; private set; }

        public override string ToString()
        {
            return ToString("");
        }

        public string ToString(String prefix)
        {
            return String.Format("{0}{1}: {2}", prefix, Key, Value.ToString());
        }
    }
}
