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
        XElement CreateTag(String tagName);

        Option<String> CreateTypeDescription();
    }

    public interface IDefaultableTaggedValueType<T> : ITaggedValueType
    {
        IDefaultableTaggedValueType<T> WithDefaultValue(T defaultValue);
    }

    public static class TaggedValueTypes
    {
        public static readonly IDefaultableTaggedValueType<String> String = new PrimitiveType<String>(typeName: "String");
        public static readonly IDefaultableTaggedValueType<int> Int = new PrimitiveType<int>(typeName: "Integer");
        public static readonly IDefaultableTaggedValueType<bool> Bool = new PrimitiveType<bool>(typeName: "Boolean");

        public static IDefaultableTaggedValueType<T> Enum<T>(IEnumerable<T> values) where T : Enumeration
        {
            return new EnumType<T>(values);
        }

        public static readonly ITaggedValueType DateTime = new StructuredType(new Dictionary<String, String> {
            {"Type", "DateTime"}
        });
        public static readonly ITaggedValueType Memo = new StructuredType(new Dictionary<String, String> {
            {"Type", "Memo"}
        });

        public static ITaggedValueType Const(String value)
        {
            return new StructuredType(new Dictionary<String, String>
            {
                {"Type", "Const"},
                {"Default", value}
            });
        }

        public static ITaggedValueType Reference(ElementStereotype stype)
        {
            return new StructuredType(new Dictionary<String, String> {
                {"Type", "RefGUID"},
                {"Values", stype.Type.Name},
                {"Stereotypes", stype.Name}
            });
        }

        public static ITaggedValueType ReferenceList(ElementStereotype stype)
        {
            return new StructuredType(new Dictionary<String, String> {
                {"Type", "RefGUIDList"},
                {"Values", stype.Type.Name},
                {"Stereotypes", stype.Name}
            });
        }
    }

    class PrimitiveType<T> : IDefaultableTaggedValueType<T>
    {
        internal PrimitiveType(String typeName, Option<T> defaultValue = null)
        {
            TypeName = typeName;
            DefaultValue = defaultValue ?? Options.None<T>();
        }

        public String TypeName { get; private set; }

        public Option<T> DefaultValue { get; private set; }

        public IDefaultableTaggedValueType<T> WithDefaultValue(T defaultValue)
        {
            return new PrimitiveType<T>(TypeName, defaultValue.AsOption());
        }

        public XElement CreateTag(string tagName)
        {
            return new XElement("Tag",
                new XAttribute("name", tagName),
                new XAttribute("type", TypeName),
                new XAttribute("description", ""),
                new XAttribute("unit", ""),
                new XAttribute("values", ""),
                new XAttribute("default", DefaultValue.Select(dv => dv.ToString()).GetOrElse("")));
        }

        public Option<string> CreateTypeDescription()
        {
            return Options.None<String>();
        }
    }

    class EnumType<T> : IDefaultableTaggedValueType<T> where T : Enumeration
    {
        internal EnumType(IEnumerable<T> values, Option<T> defaultValue = null)
        {
            Values = values;
            DefaultValue = defaultValue ?? Options.None<T>();
        }

        public IEnumerable<T> Values { get; private set; }

        public Option<T> DefaultValue { get; set; }

        public IDefaultableTaggedValueType<T> WithDefaultValue(T defaultValue)
        {
            return new EnumType<T>(Values, defaultValue.AsOption());
        }

        public XElement CreateTag(string tagName)
        {
            return new XElement("Tag",
                new XAttribute("name", tagName),
                new XAttribute("type", "enum"),
                new XAttribute("description", ""),
                new XAttribute("unit", ""),
                new XAttribute("values", Values.Select(v => v.Name).Join(",")),
                new XAttribute("default", DefaultValue.Select(dv => dv.ToString()).GetOrElse("")));
        }

        public Option<string> CreateTypeDescription()
        {
            return Options.None<String>();
        }
    }

    /// <summary>
    /// Adds support for structured tagged value types.
    /// http://www.sparxsystems.com/enterprise_architect_user_guide/9.3/standard_uml_models/predefinedtaggedvaluetypes.html
    /// </summary>
    class StructuredType : ITaggedValueType
    {
        public StructuredType(IDictionary<String, String> properties)
        {
            Properties = properties;
        }

        public IDictionary<string, string> Properties { get; set; }

        public XElement CreateTag(string tagName)
        {
            // If default value is not set, the value will not be written to the element itself but only
            // referenced in the type definition. Thus, the value would not be persistent over model changes.
            // This is especially problematic for const values.
            var defaultValue = Properties.Get("Default").GetOrElse("");

            return new XElement("Tag",
                new XAttribute("name", tagName),
                new XAttribute("type", tagName),
                new XAttribute("description", ""),
                new XAttribute("unit", ""),
                new XAttribute("values", ""),
                new XAttribute("default", defaultValue));
        }

        public Option<string> CreateTypeDescription()
        {
            return Options.Some(Properties.Select(pair => String.Format("{0}={1};", pair.Key, pair.Value)).Join(""));
        }
    }
}
