using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public static bool IsEmpty<T>(this IEnumerable<T> es)
        {
            return !es.FirstOption().IsDefined;
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

        public static IEnumerable<T> Run<T>(this IEnumerable<T> ts)
        {
            return ts.ToList();
        }

        public static Option<V> Get<K, V>(this IDictionary<K, V> dict, K key)
        {
            V value = default(V);
            if (dict.TryGetValue(key, out value))
            {
                return Options.Some(value);
            }
            else
            {
                return Options.None<V>();
            }
        }

        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> ts, Func<T, TKey> getKey)
        {
            HashSet<TKey> keys = new HashSet<TKey>();
            foreach (var t in ts)
            {
                if (keys.Add(getKey(t)))
                {
                    yield return t;
                }
            }
        }
    }
}
