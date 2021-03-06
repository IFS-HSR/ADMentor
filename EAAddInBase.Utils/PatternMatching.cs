﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInBase.Utils
{
    public static class PatternMatching
    {
        public static MatchResult<TIn, TOut> Match<TIn, TOut>(this TIn source)
        {
            return new NotMatched<TIn, TOut>(source);
        }

        public static TOut GetOrDefault<TIn, TOut>(this MatchResult<TIn, TOut> mr, TOut def)
        {
            return mr.GetOrNone().GetOrElse(def);
        }
    }

    public interface MatchResult<TIn, TOut>
    {
        MatchResult<TIn, TOut> Case<T>(Func<T, TOut> fn) where T : class, TIn;

        Option<TOut> GetOrNone();

        TOut GetOrThrowNotImplemented();

        TOut Default(Func<TIn, TOut> getDefault);
    }

    sealed class Matched<TIn, TOut> : MatchResult<TIn, TOut>
    {
        private readonly TOut Res;

        public Matched(TOut res)
        {
            Res = res;
        }

        public MatchResult<TIn, TOut> Case<T>(Func<T, TOut> fn) where T : class, TIn
        {
            return this;
        }

        public Option<TOut> GetOrNone()
        {
            return Options.Some(Res);
        }


        public TOut GetOrThrowNotImplemented()
        {
            return Res;
        }


        public TOut Default(Func<TIn, TOut> getDefault)
        {
            return Res;
        }
    }

    sealed class NotMatched<TIn, TOut> : MatchResult<TIn, TOut>
    {
        private readonly TIn Source;

        public NotMatched(TIn source)
        {
            Source = source;
        }

        public MatchResult<TIn, TOut> Case<T>(Func<T, TOut> fn) where T : class, TIn
        {
            if (Source is T)
                return new Matched<TIn, TOut>(fn(Source as T));
            else
                return this;
        }

        public Option<TOut> GetOrNone()
        {
            return Options.None<TOut>();
        }

        public TOut GetOrThrowNotImplemented()
        {
            throw new NotImplementedException(String.Format("Match is not exhaustive for type {0}", Source.GetType()));
        }

        public TOut Default(Func<TIn, TOut> getDefault)
        {
            return getDefault(Source);
        }
    }
}
