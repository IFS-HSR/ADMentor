using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInBase.Utils
{
    public interface IFilter<in T>
    {
        bool Accept(T e);

        String Name { get; }
    }

    public sealed class Filter<T> : IFilter<T>
    {
        private readonly Func<T, bool> accept;

        public Filter(String name, Func<T, bool> accept)
        {
            Name = name;
            this.accept = accept;
        }

        public string Name { get; private set; }

        public bool Accept(T e)
        {
            return accept(e);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public sealed class CompositeFilter<T> : IFilter<T>
    {
        private readonly Func<IEnumerable<IFilter<T>>, T, bool> accept;
        private readonly string OperatorName;

        internal CompositeFilter(String name, String op, IImmutableList<IFilter<T>> filters, Func<IEnumerable<IFilter<T>>, T, bool> accept)
        {
            Name = name;
            OperatorName = op;
            Filters = ImmutableList.CreateRange(filters);
            this.accept = accept;
        }

        public string Name { get; private set; }

        public IImmutableList<IFilter<T>> Filters { get; private set; }

        public CompositeFilter<T> Copy(IEnumerable<IFilter<T>> filters)
        {
            return new CompositeFilter<T>(Name, OperatorName, filters.ToImmutableList(), accept);
        }

        public bool Accept(T e)
        {
            return accept(Filters, e);
        }

        public override string ToString()
        {
            return String.Format("{0}_{1}({2})",
                Name,
                OperatorName,
                Filters.Join(", "));
        }
    }

    public static class Filter
    {
        public static IFilter<T> Create<T>(String name, Func<T, bool> accept)
        {
            return new Filter<T>(name, accept);
        }

        public static CompositeFilter<T> And<T>(String name, IEnumerable<IFilter<T>> filters)
        {
            return new CompositeFilter<T>(name, "and", filters.ToImmutableList(), (fs, e) =>
            {
                return fs.Aggregate(true, (acc, f) => acc && f.Accept(e));
            });
        }

        public static CompositeFilter<T> And<T>(String name, Func<T, bool> guard, IEnumerable<IFilter<T>> filters)
        {
            return new CompositeFilter<T>(name, "and", filters.ToImmutableList(), (fs, e) =>
            {
                return guard(e) && fs.Aggregate(true, (acc, f) => acc && f.Accept(e));
            });
        }

        public static CompositeFilter<T> Or<T>(String name, IEnumerable<IFilter<T>> filters)
        {
            return new CompositeFilter<T>(name, "or", filters.ToImmutableList(), (fs, e) =>
            {
                return fs.Aggregate(false, (acc, f) => acc || f.Accept(e));
            });
        }
    }
}
