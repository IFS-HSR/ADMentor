using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class FuncExtensions
    {
        public static Func<T, R> Memoize<T, R>(this Func<T, R> f)
        {
            var memo = new Dictionary<T, R>();
            return t =>
            {
                return Options.Try(() => memo[t]).Match(
                    r => r,
                    () =>
                    {
                        var r = f(t);
                        memo[t] = r;
                        return r;
                    });
            };
        }

        public static Func<Tuple<T1, T2>, R> Tuplify<T1, T2, R>(this Func<T1, T2, R> f)
        {
            return ts => f(ts.Item1, ts.Item2);
        }

        public static Func<T1, T2, R> Detuplify<T1, T2, R>(this Func<Tuple<T1, T2>, R> f)
        {
            return (t1, t2) => f(Tuple.Create(t1, t2));
        }

        public static Func<T1, Func<T2, R>> Curry<T1, T2, R>(this Func<T1, T2, R> f)
        {
            return t1 => t2 => f(t1, t2);
        }

        public static Func<T1, T2, R> Uncurry<T1, T2, R>(this Func<T1, Func<T2, R>> f)
        {
            return (t1, t2) => f(t1)(t2);
        }
    }
}
