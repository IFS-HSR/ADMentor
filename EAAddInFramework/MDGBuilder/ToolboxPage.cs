using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EAAddInFramework.MDGBuilder
{
    public class ToolboxPage
    {
        public ToolboxPage(String name, String displayName, String description, IEnumerable<IStereotype> stereotypes)
        {
            Name = name;
            DisplayName = displayName;
            Description = description;
            Stereotypes = stereotypes;
        }

        public String Name { get; private set; }

        public String DisplayName { get; private set; }

        public String Description { get; private set; }

        public IEnumerable<IStereotype> Stereotypes { get; private set; }

        public XElement ToXml(string technologyName)
        {
            return new XElement("Stereotype", new XAttribute("name", Name), new XAttribute("alias", DisplayName), new XAttribute("notes", Description),
                new XElement("AppliesTo",
                    new XElement("Apply", new XAttribute("type", "ToolboxPage"))),
                new XElement("TaggedValues",
                    from s in Stereotypes
                    select MakeStereotypeTag(technologyName, s)));
        }

        private XElement MakeStereotypeTag(string technologyName, IStereotype stereotype)
        {
            return new XElement("Tag",
                new XAttribute("name", String.Format("{0}::{1}(UML::{2})", technologyName, stereotype.Name, stereotype.Type.ToString())),
                new XAttribute("type", ""),
                new XAttribute("description", ""),
                new XAttribute("unit", ""),
                new XAttribute("values", ""),
                new XAttribute("default", stereotype.DisplayName));
        }
    }
}
