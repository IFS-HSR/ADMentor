using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utils;

namespace EAAddInFramework.MDGBuilder
{
    public interface ITaggedValueType
    {
        XElement MakeAttribute(String name, Option<String> description);
    }

    public class TaggedValueType : ITaggedValueType
    {
        public static readonly TaggedValueType String = new TaggedValueType(typeName: "string");
        public static readonly TaggedValueType Int = new TaggedValueType(typeName: "int");
        public static readonly TaggedValueType Bool = new TaggedValueType(typeName: "bool");

        public TaggedValueType(String typeName, object defaultValue = null)
        {
            TypeName = typeName;
            DefaultValue = defaultValue.AsOption();
        }

        public TaggedValueType WithDefaultValue(object defaultValue)
        {
            return new TaggedValueType(TypeName, defaultValue);
        }

        public String TypeName { get; private set; }
        public Option<object> DefaultValue { get; private set; }

        public XElement MakeAttribute(string name, Option<String> description)
        {
            return new XElement("Tag",
                new XAttribute("name", name),
                new XAttribute("type", TypeName),
                new XAttribute("description", description.GetOrElse("")),
                new XAttribute("unit", ""),
                new XAttribute("values", ""),
                new XAttribute("default", DefaultValue.Select(dv => dv.ToString()).GetOrElse("")));
        }
    }

    public class EnumTaggedValue : ITaggedValueType
    {

        public EnumTaggedValue(IEnumerable<Enumeration> values, Enumeration defaultValue = null)
        {
            Values = values;
            Default = defaultValue.AsOption();
        }

        public IEnumerable<Enumeration> Values { get; private set; }

        public Option<Enumeration> Default { get; private set; }

        public XElement MakeAttribute(string name, Option<String> description)
        {
            return new XElement("Tag",
                new XAttribute("name", name),
                new XAttribute("type", "enumeration"),
                new XAttribute("description", description.GetOrElse("")),
                new XAttribute("unit", ""),
                new XAttribute("values", String.Join(",", Values.Select(v => v.Name))),
                new XAttribute("default", Default.Select(v => v.Name).GetOrElse("")));
        }
    }
}
