﻿using EAAddInFramework.DataAccess;
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
        private readonly ModelEntityRepository Repo;

        private XmlExporter(XDocument document, XmlNamespaceManager namespaceManager, EA.Project projectInterface, ModelEntityRepository repo)
        {
            Document = document;
            NamespaceManager = namespaceManager;
            ProjectInterface = projectInterface;
            Repo = repo;
        }

        public void Tailor(IEnumerable<ModelEntity> entitiesToExport)
        {
            var connectors = from entity in entitiesToExport
                             from element in entity.Match<ModelEntity.Element>()
                             from connector in element.Connectors()
                             from oppositeEnd in connector.OppositeEnd(element, Repo.GetElement)
                             where entitiesToExport.Contains(oppositeEnd)
                             select connector;

            var allExportedEntities = entitiesToExport.Concat(connectors);

            Tailor(guid => allExportedEntities.Any(e => e.Guid.Equals(guid)));
        }

        public void Tailor(Func<string, bool> keep)
        {
            var modelEntities = Document.XPathSelectElements("//UML:Namespace.ownedElement/*[@xmi.id]", NamespaceManager);
            var diagramEntities = Document.XPathSelectElements("//XMI.content/UML:Diagram", NamespaceManager);

            (from entity in modelEntities.Concat(diagramEntities)
             // don't remove EA root elements
             where entity.Attribute("isRoot").AsOption().Match(a => !a.Value.Equals("true"), () => true)
             where !keep(ProjectInterface.XMLtoGUID(entity.Attribute("xmi.id").Value))
             select entity).Remove();
        }

        public void WriteTo(Stream outStream)
        {
            using (var writer = XmlWriter.Create(outStream))
            {
                Document.WriteTo(writer);
            }
        }

        public class Factory
        {
            private readonly ModelEntityRepository Repo;
            private readonly IReadableAtom<EA.Repository> EaRepo;
            public Factory(ModelEntityRepository repo, IReadableAtom<EA.Repository> eaRepo)
            {
                Repo = repo;
                EaRepo = eaRepo;
            }

            public void WithXmlExporter(ModelEntity.Package package, Action<XmlExporter> act)
            {
                var project = EaRepo.Val.GetProjectInterface();

                var tempPath = Path.GetTempFileName();
                var xmlGuid = project.GUIDtoXML(package.Guid);

                project.ExportPackageXMI(xmlGuid, EA.EnumXMIType.xmiEA11, 1, 0, 1, 0, tempPath);
                using (var reader = XmlReader.Create(tempPath))
                {
                    var doc = XDocument.Load(reader);

                    var namespaceManager = new XmlNamespaceManager(reader.NameTable);
                    namespaceManager.AddNamespace("UML", "omg.org/UML1.3");

                    var exporter = new XmlExporter(doc, namespaceManager, project, Repo);

                    act(exporter);
                }

                File.Delete(tempPath);
            }
        }
    }
}
