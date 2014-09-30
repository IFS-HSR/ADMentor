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
    public class MDGTechnology
    {
        public MDGTechnology(String id, String name, String version, String description = "",
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

        public IEnumerable<IStereotype> Stereotypes
        {
            get
            {
                return (from t in Toolboxes
                        from p in t.Pages
                        from s in p.Stereotypes
                        select s).Distinct();
            }
        }

        public IEnumerable<ConnectorStereotype> ConnectorStereotypes
        {
            get
            {
                return from s in Stereotypes
                       where s is ConnectorStereotype
                       select s as ConnectorStereotype;
            }
        }

        public IEnumerable<ElementStereotype> ElementStereotypes
        {
            get
            {
                return from s in Stereotypes
                       where s is ElementStereotype
                       select s as ElementStereotype;
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

        IEnumerable<ITaggedValue> TaggedValues
        {
            get
            {
                return (from s in Stereotypes
                        from t in s.TaggedValues
                        select t)
                        .Distinct(new TaggedValueComparer())
                        .Concat(new[]{GetModelIdTag()});
            }
        }

        private class TaggedValueComparer : IEqualityComparer<ITaggedValue>
        {
            public bool Equals(ITaggedValue x, ITaggedValue y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(ITaggedValue obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        public XDocument ToXml()
        {
            var propertyDataSetChilds = new object[]{
                new XAttribute("name", "Property Types"), new XAttribute("table", "t_propertytypes"), new XAttribute("filter", "Property='#Property#'")
            }.Concat(TaggedValues.Select(tv => tv.ToXmlType()));

            var versionTag = GetModelIdTag();

            return new XDocument(
                new XElement("MDG.Technology", new XAttribute("version", "1.0"),
                    new XElement("Documentation", new XAttribute("id", ID), new XAttribute("name", Name), new XAttribute("version", Version), new XAttribute("notes", Description)),
                    new XElement("UMLProfiles",
                        new XElement("UMLProfile", new XAttribute("profiletype", "uml2"),
                            new XElement("Documentation", new XAttribute("id", "abcdefab-1"), new XAttribute("name", ID), new XAttribute("version", Version), new XAttribute("notes", Description)),
                            new XElement("Content",
                                new XElement("Stereotypes",
                                    from s in Stereotypes
                                    select s.ToXml(versionTag)),
                                new XElement("QuickLink", new XAttribute("data", getQuickLinkData()))))),
                    new XElement("TaggedValueTypes",
                        new XElement("RefData", new XAttribute("version", "1.0"), new XAttribute("exporter", "EA.25"),
                            new XElement("DataSet", propertyDataSetChilds))),
                    new XElement("DiagramProfile",
                        new XElement("UMLProfile", new XAttribute("profiletype", "uml2"),
                            new XElement("Documentation", new XAttribute("id", "abcdefab-1"), new XAttribute("name", ID), new XAttribute("version", Version), new XAttribute("notes", Description)),
                            new XElement("Content",
                                new XElement("Stereotypes",
                                    from d in Diagrams
                                    select d.ToXml()
                                    )))),
                    new XElement("UIToolboxes",
                        from t in Toolboxes
                        select t.ToXml(ID, Version)),
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
            return quickLinkEntries.Join("\n");
        }

        internal String GetModelId()
        {
            return String.Format("{0}:{1}", ID, Version);
        }

        public static readonly String ModelIdTagName = "XModelId";

        private ITaggedValue GetModelIdTag()
        {
            return new TaggedValue(ModelIdTagName, TaggedValueTypes.Const(GetModelId()));
        }
    }
}
