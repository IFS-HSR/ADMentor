using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EAAddInBase.MDGBuilder
{
    public sealed class ToolboxPage
    {
        public ToolboxPage(String name, String displayName, String description, IEnumerable<Stereotype> stereotypes = null, IEnumerable<UMLPattern> patterns = null)
        {
            Name = name;
            DisplayName = displayName;
            Description = description;
            Stereotypes = stereotypes ?? new Stereotype[] { };
            Patterns = patterns ?? new UMLPattern[] { };
        }

        public String Name { get; private set; }

        public String DisplayName { get; private set; }

        public String Description { get; private set; }

        public IEnumerable<Stereotype> Stereotypes { get; private set; }

        public IEnumerable<UMLPattern> Patterns { get; private set; }

        internal XElement ToXml(string technologyName)
        {
            return new XElement("Stereotype", new XAttribute("name", Name), new XAttribute("alias", DisplayName), new XAttribute("notes", Description),
                new XElement("AppliesTo",
                    new XElement("Apply", new XAttribute("type", "ToolboxPage"))),
                new XElement("TaggedValues",
                    (from s in Stereotypes
                     select MakeStereotypeTag(technologyName, s))
                    .Concat(
                    from p in Patterns
                    select MakePatternTag(technologyName, p))));
        }

        private XElement MakeStereotypeTag(string technologyName, Stereotype stereotype)
        {
            return new XElement("Tag",
                new XAttribute("name", String.Format("{0}::{1}(UML::{2})", technologyName, stereotype.Name, stereotype.Type.ToString())),
                new XAttribute("type", ""),
                new XAttribute("description", ""),
                new XAttribute("unit", ""),
                new XAttribute("values", ""),
                new XAttribute("default", stereotype.DisplayName));
        }

        private XElement MakePatternTag(string technologyName, UMLPattern pattern)
        {

            return new XElement("Tag",
                new XAttribute("name", String.Format("{0}::{1}(UMLPattern)", technologyName, pattern.Name)),
                new XAttribute("type", ""),
                new XAttribute("description", ""),
                new XAttribute("unit", ""),
                new XAttribute("values", ""),
                new XAttribute("default", pattern.DisplayName));
        }
    }
}
