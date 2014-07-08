using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> es, Action<T> action){
            foreach (var e in es)
            {
                action(e);
            }
        }
    }
}
