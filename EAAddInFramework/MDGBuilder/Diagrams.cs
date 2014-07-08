using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EAAddInFramework.MDGBuilder
{
    public class Diagram
    {
        public Diagram(String name, String displayName, String description, DiagramType type, Toolbox toolbox)
        {
            Name = name;
            DisplayName = displayName;
            Description = description;
            Type = type;
            Toolbox = toolbox;
        }

        public String Name { get; private set; }

        public String DisplayName { get; private set; }

        public String Description { get; private set; }

        public DiagramType Type { get; private set; }

        public Toolbox Toolbox { get; private set; }

        public XElement ToXml()
        {
            return new XElement("Stereotype", new XAttribute("name", Name), new XAttribute("alias", DisplayName), new XAttribute("notes", Description),
                new XElement("AppliesTo",
                    new XElement("Apply", new XAttribute("type", "Diagram_" + Type.ToString()),
                        new XElement("Property", new XAttribute("name", "alias"), new XAttribute("value", DisplayName)),
                        new XElement("Property", new XAttribute("name", "diagramID"), new XAttribute("value", Name)),
                        new XElement("Property", new XAttribute("name", "toolbox"), new XAttribute("value", Toolbox.Name)),
                        new XElement("Property", new XAttribute("name", "_metamodel"), new XAttribute("value", Name)))));
        }
    }
}
