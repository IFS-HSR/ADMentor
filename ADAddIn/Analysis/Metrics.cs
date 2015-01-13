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

        public static Metric Entry(String key, Object value)
        {
            return new Entry(key, value);
        }
    }

    public interface Metric { }

    public class Category : Metric
    {
        public Category(String name, IEnumerable<Metric> members)
        {
            Name = name;
            Members = members;
        }

        public string Name { get; private set; }

        public IEnumerable<Metric> Members { get; private set; }
    }

    public class Entry : Metric
    {
        public Entry(String key, Object value)
        {
            Key = key;
            Value = value.ToString();
        }

        public string Key { get; private set; }

        public string Value { get; private set; }
    }
}
