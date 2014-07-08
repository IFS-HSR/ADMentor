using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EAAddInFramework.MDGBuilder
{
    public class MDGTechnology
    {
        public MDGTechnology(String id, String name, String version = "1.0.0", String description = "",
            IEnumerable<Diagram> diagrams = null, IEnumerable<ModelTemplate> modelTemplates = null)
        {
            ID = id;
            Name = name;
            Version = version;
            Description = description;
            Diagrams = diagrams ?? new Diagram[] { };
            ModelTemplates = modelTemplates ?? new ModelTemplate[] { };
        }

        public string ID { get; private set; }

        public string Name { get; private set; }

        public string Version { get; private set; }

        public string Description { get; private set; }

        public IEnumerable<Diagram> Diagrams { get; private set; }

        public IEnumerable<ModelTemplate> ModelTemplates { get; private set; }

        IEnumerable<IStereotype> Stereotypes
        {
            get
            {
                return (from t in Toolboxes
                        from p in t.Pages
                        from s in p.Stereotypes
                        select s).Distinct();
            }
        }

        IEnumerable<Toolbox> Toolboxes
        {
            get
            {
                return (from d in Diagrams
                        select d.Toolbox).Distinct();
            }
        }

        public XDocument ToXml()
        {
            return new XDocument(
                new XElement("MDG.Technology", new XAttribute("version", 1),
                    new XElement("Documentation", new XAttribute("id", ID), new XAttribute("name", Name), new XAttribute("version", Version), new XAttribute("notes", Description)),
                    new XElement("UMLProfiles",
                        new XElement("UMLProfile", new XAttribute("profiletype", "uml2"),
                            new XElement("Documentation", new XAttribute("id", "abcdefab-1"), new XAttribute("name", Name), new XAttribute("version", Version), new XAttribute("notes", Description)),
                            new XElement("Content",
                                new XElement("Stereotypes",
                                    from s in Stereotypes
                                    select s.ToXml()),
                                new XElement("QuickLink", new XAttribute("data", getQuickLinkData()))))),
                    new XElement("DiagramProfile",
                        new XElement("UMLProfile", new XAttribute("profiletype", "uml2"),
                            new XElement("Documentation", new XAttribute("id", "abcdefab-1"), new XAttribute("name", Name), new XAttribute("version", Version), new XAttribute("notes", Description)),
                            new XElement("Content",
                                new XElement("Stereotypes",
                                    from d in Diagrams
                                    select d.ToXml()
                                    )))),
                    new XElement("UIToolboxes",
                        from t in Toolboxes
                        select t.ToXml(Name, Version)),
                    new XElement("ModelTemplates",
                        from t in ModelTemplates
                        select t.ToXml())));
        }

        private String getQuickLinkData()
        {
            var quickLinkEntries = from s in Stereotypes
                                   where s is ConnectorStereotype
                                   from c in (s as ConnectorStereotype).Connects
                                   from ql in c.GetQuickLinkEntries()
                                   select ql;
            return String.Join("\n", quickLinkEntries);
        }
    }
}
