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
    public class TaggedValue
    {
        public TaggedValue(String name, ITaggedValueType type, String description = null)
        {
            Name = name;
            Description = description.AsOption();
            Type = type;
        }

        public string Name { get; private set; }

        public Option<string> Description { get; private set; }

        public ITaggedValueType Type { get; private set; }

        public XElement ToXml()
        {
            return Type.MakeAttribute(Name, Description);
        }
    }
}
