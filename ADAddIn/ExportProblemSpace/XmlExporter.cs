using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using System.Xml.Linq;
using Utils;
using System.Xml;

namespace AdAddIn.ExportProblemSpace
{
    public class XmlExporter
    {
        private readonly XDocument Document;
        private readonly XmlNamespaceManager NamespaceManager;
        private readonly EA.Project ProjectInterface;

        private XmlExporter(XDocument document, XmlNamespaceManager namespaceManager, EA.Project projectInterface)
        {
            Document = document;
            NamespaceManager = namespaceManager;
            ProjectInterface = projectInterface;
        }

        public void RemoveEntities(Func<string, bool> remove)
        {
            var modelEntities = Document.XPathSelectElements("//UML:Namespace.ownedElement/*[@xmi.id]", NamespaceManager);
            var diagramEntities = Document.XPathSelectElements("//XMI.content/UML:Diagram", NamespaceManager);

            (from entity in modelEntities.Concat(diagramEntities)
             where remove(ProjectInterface.XMLtoGUID(entity.Attribute("xmi.id").Value))
             select entity).Remove();
        }

        public void WriteTo(string path)
        {
            using (var writer = XmlWriter.Create(path))
            {
                Document.WriteTo(writer);
            }
        }

        public class Factory
        {
            private readonly IReadableAtom<EA.Repository> Repo;
            public Factory(IReadableAtom<EA.Repository> repo)
            {
                Repo = repo;
            }

            private EA.Project Project { get { return Repo.Val.GetProjectInterface(); } }

            public void WithXmlExporter(ModelEntity.Package package, Action<XmlExporter> act)
            {
                var tempPath = Path.GetTempFileName();
                var xmlGuid = Project.GUIDtoXML(package.Guid);

                Project.ExportPackageXMI(xmlGuid, EA.EnumXMIType.xmiEA11, 1, 0, 1, 0, tempPath);
                using (var reader = XmlReader.Create(tempPath))
                {
                    var doc = XDocument.Load(reader);

                    var namespaceManager = new XmlNamespaceManager(reader.NameTable);
                    namespaceManager.AddNamespace("UML", "omg.org/UML1.3");

                    var exporter = new XmlExporter(doc, namespaceManager, Repo.Val.GetProjectInterface());

                    act(exporter);
                }

                File.Delete(tempPath);
            }
        }
    }
}
