using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using EAAddInFramework;
using EAAddInFramework.MDGBuilder;
using AdAddIn.ADTechnology;

namespace AdAddIn.DataAccess
{
    /// <summary>
    /// Provides CRUD operations and further methods for EA elements and connectors.
    /// </summary>
    public class ElementRepository
    {
        private readonly IReadableAtom<EA.Repository> EA;

        public ElementRepository(IReadableAtom<EA.Repository> ea)
        {
            EA = ea;
        }

        public MDGTechnology Technology { get { return ADTechnology.Technologies.AD; } }

        public Option<EA.Element> GetElement(int id)
        {
            return EA.Val.TryGetElement(id);
        }

        public Option<EA.Element> GetElement(string guid)
        {
            return EA.Val.TryGetElement(guid);
        }

        public Option<EA.Element> GetClassifier(EA.Element e)
        {
            return from c in EA.Val.TryGetElement(e.ClassifierID)
                   select c;
        }

        public Option<ElementStereotype> GetStereotype(EA.Element e)
        {
            return (from stype in Technology.Stereotypes
                    where stype is ElementStereotype
                    where stype.Name == e.Stereotype
                    select stype as ElementStereotype).FirstOption();
        }

        public Option<ConnectorStereotype> GetStereotype(EA.Connector e)
        {
            return (from stype in Technology.Stereotypes
                    where stype is ConnectorStereotype
                    where stype.Name == e.Stereotype
                    select stype as ConnectorStereotype).FirstOption();
        }

        public EA.Package FindPackageContaining(EA.Element e)
        {
            return e.FindPackage(EA.Val);
        }

        public Utils.Option<EA.Element> Instanciate(EA.Element classifier, EA.Package package)
        {
            return from stype in GetStereotype(classifier)
                   from instance in stype.Instanciate(classifier, package)
                   let _ = UpdateMetadata(instance)
                   select instance;
        }

        public EntityModified UpdateMetadata(EA.Element element)
        {
            Func<EA.Element, EA.Element, EntityModified> update = (e, classifier) =>
            {
                e.Notes = classifier.Notes;
                e.Update();
                return EntityModified.Modified;
            };

            return (from c in GetClassifier(element)
                    where c.Is(ElementStereotypes.Problem) || c.Is(ElementStereotypes.Option)
                    select update(element, c)).GetOrElse(EntityModified.NotModified);
        }
    }
}
