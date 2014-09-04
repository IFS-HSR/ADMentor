using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    [TypeConverter(typeof(OptionConverter))]
    public interface Option<out T> : IEnumerable<T>
    {
        bool IsDefined { get; }
        T Value { get; }
        T2 Match<T2>(Func<T, T2> then, Func<T2> els);
    }

    class OptionImpl<T> : Option<T>
    {
        private readonly T value;
        private readonly bool isDefined;

        internal OptionImpl()
        {
            value = default(T);
            isDefined = false;
        }

        internal OptionImpl(T v)
        {
            value = v;
            isDefined = true;
        }

        internal static readonly Option<T> None = new OptionImpl<T>();

        public bool IsDefined
        {
            get { return isDefined; }
        }

        public T Value
        {
            get
            {
                if (isDefined)
                    return value;
                else
                    throw new InvalidOperationException("Get Value of None");
            }
        }

        public T2 Match<T2>(Func<T, T2> then, Func<T2> els)
        {
            if (isDefined)
                return then(Value);
            else
                return els();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ToEnumerable().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<T> ToEnumerable()
        {
            if (isDefined)
                yield return Value;
            else
                yield break;
        }

        public override string ToString()
        {
            return Match(
                v => String.Format("Some({0})", v),
                () => "None");
        }

        public override int GetHashCode()
        {
            return Match(
                v => (397 + v.GetHashCode()) * 17,
                () => 0);
        }

        public override bool Equals(object other)
        {
            return other.TryCast<Option<T>>().Match(
                opt => IsDefined == opt.IsDefined && opt.Match(
                    val => Value.Equals(val),
                    () => true),
                () => false);
        }
    }

    public static class Options
    {
        public static Option<T> Some<T>(T v)
        {
            return new OptionImpl<T>(v);
        }

        public static Option<T> None<T>()
        {
            return OptionImpl<T>.None;
        }

        public static Option<T> Try<T>(Func<T> fn)
        {
            try
            {
                return fn().AsOption();
            }
            catch
            {
                return Options.None<T>();
            }
        }
    }

    public static class OptionExtensions
    {
        public static Option<T> AsOption<T>(this Nullable<T> value) where T : struct
        {
            if (value.HasValue)
                return Options.Some(value.Value);
            else
                return Options.None<T>();
        }

        public static Option<T> AsOption<T>(this T value)
        {
            if (value != null)
                return Options.Some(value);
            else
                return Options.None<T>();
        }

        public static void Do<T>(this Option<T> opt, Action<T> action)
        {
            if (opt.IsDefined)
                action(opt.Value);
        }

        public static T GetOrElse<T>(this Option<T> opt, T els)
        {
            return opt.Match(
                v => v,
                () => els);
        }

        public static Option<T> OrElse<T>(this Option<T> opt, Option<T> els)
        {
            return opt.Match(
                v => opt,
                () => els);
        }

        public static T GetOrDefault<T>(this Option<T> opt)
        {
            return opt.Match(
                v => v,
                () => default(T));
        }

        public static Option<TResult> Select<T, TResult>(this Option<T> opt, Func<T, TResult> fn)
        {
            return opt.Match(
                v => Options.Some(fn(v)),
                () => Options.None<TResult>());
        }

        public static Option<TResult> SelectMany<T, TResult>(this Option<T> opt, Func<T, Option<TResult>> fn)
        {
            return opt.Match(
                v => fn(v),
                () => Options.None<TResult>());
        }

        public static Option<TResult> SelectMany<T, TCollection, TResult>(this Option<T> opt,
            Func<T, Option<TCollection>> fn, Func<T, TCollection, TResult> select)
        {
            return opt.Match(
                vColl => fn(vColl).Match(
                        vRes => Options.Some(select(vColl, vRes)),
                        () => Options.None<TResult>()),
                () => Options.None<TResult>());
        }

        public static Option<T> Where<T>(this Option<T> opt, Func<T, bool> pred)
        {
            return opt.SelectMany(v =>
            {
                if (pred(v))
                    return Options.Some(v);
                else
                    return Options.None<T>();
            });
        }

        public static Option<T> TryCast<T>(this object o) where T : class
        {
            if (o is T)
                return Options.Some(o as T);
            else
                return Options.None<T>();
        }
    }

    public static class EnumerableOptionUtils
    {
        public static Option<T> FirstOption<T>(this IEnumerable<T> elements)
        {
            return elements.FirstOption(e => true);
        }

        public static Option<T> FirstOption<T>(this IEnumerable<T> elements, Func<T, Boolean> predicate)
        {
            foreach (var e in elements)
            {
                if (predicate(e))
                {
                    return Options.Some(e);
                }
            }
            return Options.None<T>();
        }
    }

    public static class BooleanOptionUtils
    {
        public static Option<T> Then<T>(this Boolean condition, Func<T> then)
        {
            if (condition)
            {
                return Options.Some(then());
            }
            else
            {
                return Options.None<T>();
            }
        }

        public static T Else<T>(this Option<T> opt, Func<T> els)
        {
            return opt.Match(
                v => v,
                () => els());
        }
    }
}
