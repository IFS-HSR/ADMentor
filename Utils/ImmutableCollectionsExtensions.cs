using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class ImmutableCollectionsExtensions
    {
        public static IImmutableSet<T> AddRange<T>(this IImmutableSet<T> set, IEnumerable<T> range)
        {
            return range.Aggregate(set, (acc, item) => acc.Add(item));
        }

        public static ILookup<K, V> Merge<K, V>(this ILookup<K, V> l, ILookup<K, V> r)
        {
            return l.Concat(r)
                .SelectMany(grp => grp.Select(v => Tuple.Create(grp.Key, v)))
                .Distinct()
                .ToLookup(kv => kv.Item1, kv => kv.Item2);
        }
    }
}
