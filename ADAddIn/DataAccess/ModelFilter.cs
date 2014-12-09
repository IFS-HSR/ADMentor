using EAAddInFramework.DataAccess;
using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.DataAccess
{
    public abstract class ModelFilter
    {
        public abstract bool Accept(ModelEntity e);

        public abstract String Name { get; }

        public abstract class Composite : ModelFilter
        {
            public Composite(IEnumerable<ModelFilter> filters)
            {
                Filters = filters;
            }

            public IEnumerable<ModelFilter> Filters { get; private set; }

            public abstract Composite Copy(IEnumerable<ModelFilter> queries);
        }

        public class And : Composite
        {
            public And(params ModelFilter[] filters) : base(filters) { }

            public override bool Accept(ModelEntity e)
            {
                return Filters.All(q => q.Accept(e));
            }

            public override String Name { get { return "And"; } }

            public override Composite Copy(IEnumerable<ModelFilter> filters)
            {
                return new And(filters.ToArray());
            }
        }

        public class Or : Composite
        {
            public Or(params ModelFilter[] filters) : base(filters) { }

            public override bool Accept(ModelEntity e)
            {
                return Filters.Any(q => q.Accept(e));
            }

            public override String Name { get { return "Or"; } }

            public override Composite Copy(IEnumerable<ModelFilter> filters)
            {
                return new Or(filters.ToArray());
            }
        }

        public class Any : ModelFilter
        {
            public override bool Accept(ModelEntity _)
            {
                return true;
            }

            public override String Name { get { return "Any"; } }
        }

        public class BinaryFilter : ModelFilter
        {
            public BinaryFilter(Field<String> field, Operator op, String value)
            {
                Field = field;
                Value = value;
                Operator = op;
            }

            public Field<string> Field { get; private set; }

            public string Value { get; private set; }

            public Operator Operator { get; private set; }

            public override bool Accept(ModelEntity e)
            {
                return Operator.Apply(Value, Field.GetValue(e));
            }

            public override string Name { get { return String.Format("{0} {1} \"{2}\"", Field, Operator, Value); } }
        }
    }

    public abstract class Field<T>
    {
        public abstract T GetValue(ModelEntity e);

        public virtual String Name { get { return GetType().Name; } }

        public override string ToString()
        {
            return Name;
        }

        public class ElementName : Field<string>
        {
            public override string GetValue(ModelEntity e)
            {
                return e.Name;
            }

            public override String Name { get { return "Name"; } }
        }

        public class Type : Field<string>
        {
            public override string GetValue(ModelEntity e)
            {
                return e.Type;
            }
        }

        public class Stereotype : Field<String>
        {
            public override string GetValue(ModelEntity e)
            {
                return e.Stereotype;
            }
        }

        public class Keywords : Field<IEnumerable<String>>
        {
            public override IEnumerable<string> GetValue(ModelEntity e)
            {
                return e.Keywords;
            }
        }

        public class TaggedValueField : Field<String>
        {
            public TaggedValueField(TaggedValue tag)
            {
                Tag = tag;
            }

            public TaggedValue Tag { get; private set; }

            public override String GetValue(ModelEntity e)
            {
                return e.Get(Tag).GetOrElse("");
            }

            public override String Name { get { return Tag.Name; } }
        }
    }

    public abstract class Operator
    {
        public abstract bool Apply(String expected, String actual);

        public override string ToString()
        {
            return GetType().Name;
        }

        public class Is : Operator
        {
            public override bool Apply(String expected, String actual)
            {
                return expected.Equals(actual);
            }
        }

        public class Matches : Operator
        {
            public override bool Apply(string expected, string actual)
            {
                var pattern = new Regex(expected, RegexOptions.IgnoreCase);
                return pattern.IsMatch(actual);
            }
        }
    }

    public static class FilterExtensions
    {
        public static Option<ModelFilter> FindParent(this ModelFilter root, ModelFilter inner)
        {
            return root.TryCast<ModelFilter.Composite>().Match(
                composite =>
                {
                    if (composite.Filters.Contains(inner)) return Options.Some(root);
                    else return (from child in composite.Filters
                                 from parent in child.FindParent(inner)
                                 select parent).FirstOption();
                },
                () => Options.None<ModelFilter>());
        }

        public static ModelFilter Replace(this ModelFilter root, ModelFilter original, ModelFilter replacement)
        {
            return Normalize(_Replace(root, original, replacement));
        }

        private static ModelFilter _Replace(ModelFilter root, ModelFilter original, ModelFilter replacement)
        {
            if (root.Equals(original))
                return replacement;
            else
                return root.TryCast<ModelFilter.Composite>().Match(
                    composite =>
                    {
                        var newChildren = composite.Filters.Select(q => _Replace(q, original, replacement));
                        return composite.Copy(newChildren);
                    },
                    () =>
                    {
                        return root;
                    });
        }

        private static ModelFilter Normalize(ModelFilter filter)
        {
            return filter.TryCast<ModelFilter.Composite>().Match(
                compositeFilter =>
                {
                    var children = filter.Match<ModelFilter, IImmutableList<ModelFilter>>()
                        .Case<ModelFilter.Or>(or =>
                            or.Filters.Aggregate(ImmutableList.Create<ModelFilter>(), (acc, child) =>
                                child.TryCast<ModelFilter.Or>().Match(
                                    orChild => acc.AddRange((orChild).Filters),
                                    () => acc.Add(child))))
                        .Case<ModelFilter.And>(and =>
                            and.Filters.Aggregate(ImmutableList.Create<ModelFilter>(), (acc, child) =>
                                child.TryCast<ModelFilter.And>().Match(
                                    andChild => acc.AddRange((andChild).Filters),
                                    () => acc.Add(child))))
                        .GetOrThrowNotImplemented();

                    var normalizedChildren = children.Select(Normalize);

                    if (normalizedChildren.Count() == 1)
                        return normalizedChildren.ElementAt(0);
                    else
                        return compositeFilter.Copy(normalizedChildren);
                },
                () =>
                {
                    return filter;
                });
        }

        public static ModelFilter AddAlternative(this ModelFilter root, ModelFilter selected, ModelFilter newAlternative)
        {
            return root.Replace(selected, new ModelFilter.Or(selected, newAlternative));
        }

        public static ModelFilter AddRestriction(this ModelFilter root, ModelFilter selected, ModelFilter newRestriction)
        {
            return root.Replace(selected, new ModelFilter.And(selected, newRestriction));
        }

        public static ModelFilter Remove(this ModelFilter root, ModelFilter filterToRemove)
        {
            return Normalize(_Remove(root, filterToRemove));
        }

        private static ModelFilter _Remove(ModelFilter root, ModelFilter filterToRemove)
        {
            if (filterToRemove.Equals(root))
                return new ModelFilter.Any();
            else
                return root.TryCast<ModelFilter.Composite>().Match(
                    comp =>
                    {
                        var newChildren = comp.Filters.SelectMany<ModelFilter, ModelFilter>(f =>
                        {
                            if (f.Equals(filterToRemove))
                                return new ModelFilter[] { };
                            else
                                return new[] { _Remove(f, filterToRemove) };
                        });
                        return comp.Copy(newChildren);
                    },
                    () =>
                    {
                        return root;
                    });
        }
    }
}
