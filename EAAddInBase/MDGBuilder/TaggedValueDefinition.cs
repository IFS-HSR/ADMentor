using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using EAAddInBase.Utils;

namespace EAAddInBase.MDGBuilder
{
    public sealed class TaggedValueDefinition
    {
        public TaggedValueDefinition(String name, ITaggedValueType type, String description = null)
        {
            Name = name;
            Type = type;
            Description = description.AsOption();
        }

        public string Name { get; private set; }

        public Option<string> Description { get; private set; }

        public ITaggedValueType Type { get; private set; }

        internal XElement ToXml()
        {
            return Type.CreateTag(Name);
        }

        internal Option<XElement> ToXmlType()
        {
            return Type.CreateTypeDescription().Select(notes =>
            {
                return new XElement("DataRow",
                    new XElement("Column",
                        new XAttribute("name", "Property"), new XAttribute("value", Name)),
                    new XElement("Column",
                        new XAttribute("name", "Notes"), new XAttribute("value", notes)));
            });
        }
    }
}
