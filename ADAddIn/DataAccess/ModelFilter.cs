using EAAddInFramework.DataAccess;
using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
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

        public class Is : ModelFilter
        {
            public Is(Field<String> field, String value)
            {
                Field = field;
                Value = value;
            }

            public Field<String> Field { get; private set; }

            public String Value { get; private set; }

            public override bool Accept(ModelEntity e)
            {
                return Value.Equals(Field.GetValue(e));
            }

            public override String Name { get { return String.Format("{0} is {1}", Field.Name, Value); } }
        }

        public class Contains : ModelFilter
        {
            public Contains(Field<IEnumerable<String>> field, String value)
            {
                Field = field;
                Value = value;
            }

            public Field<IEnumerable<String>> Field { get; private set; }

            public string Value { get; private set; }

            public override bool Accept(ModelEntity e)
            {
                var fieldValues = Field.GetValue(e);
                return fieldValues.Any(v => v.Equals(Value));
            }

            public override String Name { get { return String.Format("{0} contains {1}", Field.Name, Value); } }
        }

        public class Matches : ModelFilter
        {
            public Matches(Field<String> field, String value)
            {
                Field = field;
                Value = value;
            }

            public Field<String> Field { get; private set; }

            public String Value { get; private set; }

            public override bool Accept(ModelEntity e)
            {
                var pattern = new Regex(Value, RegexOptions.IgnoreCase);
                return pattern.IsMatch(Field.GetValue(e));
            }

            public override String Name { get { return String.Format("{0} matches {1}", Field.Name, Value); } }
        }
    }

    public abstract class Field<T>
    {
        public abstract T GetValue(ModelEntity e);

        public abstract String Name { get; }

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
                return e.Stereotype;
            }

            public override String Name { get { return "Type"; } }
        }

        public class Keywords : Field<IEnumerable<String>>
        {
            public override IEnumerable<string> GetValue(ModelEntity e)
            {
                return e.Keywords;
            }

            public override String Name { get { return "Keywords"; } }
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

    public static class FilterExtensions
    {
        public static Option<ModelFilter> FindParent(this ModelFilter outer, ModelFilter inner)
        {
            return outer.Match<ModelFilter.Composite>().Match(
                composite =>
                {
                    if (composite.Filters.Contains(inner)) return Options.Some(outer);
                    else return (from child in composite.Filters
                                 from parent in child.FindParent(inner)
                                 select parent).FirstOption();
                },
                () => Options.None<ModelFilter>());
        }

        public static ModelFilter Replace(this ModelFilter outer, ModelFilter original, ModelFilter replacement)
        {
            return outer.Match<ModelFilter.Composite>().Match(
                composite =>
                {
                    var newChildren = composite.Filters.Select(q => q.Replace(original, replacement));
                    return composite.Copy(newChildren);
                },
                () =>
                {
                    if (outer.Equals(original))
                        return replacement;
                    else
                        return outer;
                });
        }

        public static ModelFilter AddAlternative(this ModelFilter outer, ModelFilter selected, ModelFilter newAlternative)
        {
            var parentOfSelected = outer.FindParent(selected).GetOrElse(outer);

            var res = parentOfSelected.Match<ModelFilter.Or>().Select(or =>
                or.Copy(or.Filters.Concat(new[] { newAlternative })));

            return res.GetOrElse(
                () => outer.Replace(selected, new ModelFilter.Or(selected, newAlternative)));
        }

        public static ModelFilter AddRestriction(this ModelFilter outer, ModelFilter selected, ModelFilter newRestriction)
        {
            var parentOfSelected = outer.FindParent(selected).GetOrElse(outer);

            var res = parentOfSelected.Match<ModelFilter.And>().Select(and =>
                        and.Copy(and.Filters.Concat(new[] { newRestriction })));

            return res.GetOrElse(
                () => outer.Replace(selected, new ModelFilter.And(selected, newRestriction)));
        }
    }
}
