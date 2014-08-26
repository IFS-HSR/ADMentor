using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> es, Action<T> action)
        {
            foreach (var e in es)
            {
                action(e);
            }
        }

        public static void ForEach<T, S>(this IEnumerable<Tuple<T, S>> pairs, Action<T, S> action)
        {
            foreach (var p in pairs)
            {
                action(p.Item1, p.Item2);
            }
        }

        public static void ForEach<T, S, O>(this IEnumerable<Tuple<T, S, O>> pairs, Action<T, S, O> action)
        {
            foreach (var p in pairs)
            {
                action(p.Item1, p.Item2, p.Item3);
            }
        }

        public static String Join<T>(this IEnumerable<T> es, String separator)
        {
            return String.Join(separator, es);
        }

        public static IEnumerable<Tuple<T, S>> Zip<T, S>(this IEnumerable<T> ts, IEnumerable<S> ss)
        {
            return ts.Zip(ss, (t, s) => Tuple.Create(t, s));
        }

        public static IDictionary<T, S> ToDictionary<T, S>(this IEnumerable<Tuple<T, S>> pairs)
        {
            return pairs.ToDictionary(p => p.Item1, p => p.Item2);
        }

        public static bool In<T, U>(this T item, params U[] items) where T : U
        {
            return items.Any(i => i.Equals(item));
        }
    }
}
