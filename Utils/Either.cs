using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public abstract class Either<L, R>
    {
        internal Either() { }

        public abstract T Match<T>(Func<L, T> left, Func<R, T> right);

        public static implicit operator Either<L,R>(L l)
        {
            return Either.Left<L, R>(l);
        }

        public static implicit operator Either<L, R>(R r)
        {
            return Either.Right<L, R>(r);
        }
    }

    public static class Either
    {
        public static Either<L, R> Left<L, R>(L val)
        {
            return new LeftImpl<L, R>(val);
        }

        public static Either<L, R> Right<L, R>(R val)
        {
            return new RightImpl<L, R>(val);
        }

        private class LeftImpl<L, R> : Either<L, R>
        {
            private readonly L Val;

            public LeftImpl(L val)
            {
                Val = val;
            }

            public override T Match<T>(Func<L, T> left, Func<R, T> right)
            {
                return left(Val);
            }
        }

        private class RightImpl<L, R> : Either<L, R>
        {
            private readonly R Val;

            public RightImpl(R val)
            {
                Val = val;
            }

            public override T Match<T>(Func<L, T> left, Func<R, T> right)
            {
                return right(Val);
            }
        }

        public static Option<L> Left<L, R>(this Either<L, R> e)
        {
            return e.Match(l => Options.Some(l), _ => Options.None<L>());
        }

        public static Option<R> Right<L, R>(this Either<L, R> e)
        {
            return e.Match(_ => Options.None<R>(), r => Options.Some(r));
        }

        public static dynamic Dynamic<L, R>(this Either<L, R> e)
        {
            return e.Match(l => l as dynamic, r => r as dynamic);
        }
    }
}
