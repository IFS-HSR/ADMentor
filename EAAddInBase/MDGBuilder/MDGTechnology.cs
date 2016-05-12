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
    public sealed class MDGTechnology
    {
        public MDGTechnology(String id, String name, String version, int modelVersion, String description = "",
            IEnumerable<Diagram> diagrams = null, IEnumerable<ModelTemplate> modelTemplates = null)
        {
            ID = id;
            Name = name;
            Version = version;
            ModelVersion = modelVersion;
            Description = description;
            Diagrams = diagrams ?? new Diagram[] { };
            ModelTemplates = modelTemplates ?? new ModelTemplate[] { };

            ModelId = new ModelId(ID, ModelVersion);
            ModelIdTag = new TaggedValueDefinition("XModelId", TaggedValueTypes.Const(ModelId.ToString()));
        }

        public string ID { get; private set; }

        public string Name { get; private set; }

        public string Version { get; private set; }

        public int ModelVersion { get; private set; }

        public ModelId ModelId { get; private set; }

        public TaggedValueDefinition ModelIdTag { get; private set; }

        public string Description { get; private set; }

        public IEnumerable<Diagram> Diagrams { get; private set; }

        public IEnumerable<ModelTemplate> ModelTemplates { get; private set; }

        public IEnumerable<UMLPattern> Patterns
        {
            get
            {
                return (from t in Toolboxes
                        from p in t.Pages
                        from pat in p.Patterns
                        select pat).Distinct();
            }
        }

        public IEnumerable<Stereotype> Stereotypes
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
                       from c in s.TryCast<ConnectorStereotype>()
                       select c;
            }
        }

        public IEnumerable<ElementStereotype> ElementStereotypes
        {
            get
            {
                return from s in Stereotypes
                       from e in s.TryCast<ElementStereotype>()
                       select e;
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

        public IEnumerable<TaggedValueDefinition> TaggedValues
        {
            get
            {
                return (from s in Stereotypes
                        from t in s.TaggedValues
                        select t)
                        .Distinct(new TaggedValueComparer())
                        .Concat(new[] { ModelIdTag });
            }
        }

        private class TaggedValueComparer : IEqualityComparer<TaggedValueDefinition>
        {
            public bool Equals(TaggedValueDefinition x, TaggedValueDefinition y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(TaggedValueDefinition obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        internal XDocument ToXml()
        {
            var propertyDataSetChilds = new object[]{
                new XAttribute("name", "Property Types"), new XAttribute("table", "t_propertytypes"), new XAttribute("filter", "Property='#Property#'")
            }.Concat(TaggedValues.Select(tv => tv.ToXmlType()));

            return new XDocument(
                new XElement("MDG.Technology", new XAttribute("version", "1.0"),
                    new XElement("Documentation", new XAttribute("id", ID), new XAttribute("name", Name), new XAttribute("version", Version), new XAttribute("notes", Description)),
                    new XElement("UMLProfiles",
                        new XElement("UMLProfile", new XAttribute("profiletype", "uml2"),
                            new XElement("Documentation", new XAttribute("id", "abcdefab-1"), new XAttribute("name", ID), new XAttribute("version", Version), new XAttribute("notes", Description)),
                            new XElement("Content",
                                new XElement("Stereotypes",
                                    from s in Stereotypes
                                    select s.ToXml(ModelIdTag)),
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
                    new XElement("UMLPatterns", 
                        from p in Patterns
                        select p.ToXml()),
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
    }
}
