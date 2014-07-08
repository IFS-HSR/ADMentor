using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EAAddInFramework.MDGBuilder
{
    public class Toolbox
    {
        public Toolbox(String name, String displayName, String description, IEnumerable<ToolboxPage> pages)
        {
            Name = name;
            DisplayName = displayName;
            Description = description;
            Pages = pages;
        }

        public String Name { get; private set; }

        public String DisplayName { get; private set; }

        public String Description { get; private set; }

        public IEnumerable<ToolboxPage> Pages { get; private set; }

        public XElement ToXml(string technologyName, string version)
        {
            return new XElement("UMLProfile", new XAttribute("profiletype", "uml2"),
                            new XElement("Documentation", new XAttribute("id", "abcdefab-1"), new XAttribute("name", Name), new XAttribute("alias", DisplayName), new XAttribute("version", version), new XAttribute("notes", Description)),
                            new XElement("Content",
                                new XElement("Stereotypes",
                                    from tp in Pages
                                    select tp.ToXml(technologyName))));
        }
    }
}
