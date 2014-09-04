using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdAddIn
{
    public static class ImmutableCollectionsExtensions
    {
        public static IImmutableSet<T> AddRange<T>(this IImmutableSet<T> set, IEnumerable<T> range)
        {
            return range.Aggregate(set, (acc, item) => acc.Add(item));
        }
    }
}
