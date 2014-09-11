using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace EAAddInFramework.DataAccess
{
    public class ModelEntityRepository
    {
        private readonly IReadableAtom<EA.Repository> Repo;

        public ModelEntityRepository(IReadableAtom<EA.Repository> repo, IEntityWrapper wrapper)
        {
            Repo = repo;
            Wrapper = wrapper;
        }

        protected IEntityWrapper Wrapper { get; private set; }

        public Option<ModelEntity.Element> GetElement(int id)
        {
            return from e in Options.Try(() => Repo.Val.GetElementByID(id))
                   select Wrapper.Wrap(e);
        }

        public Option<ModelEntity.Element> GetElement(string guid)
        {
            return from e in Options.Try(() => Repo.Val.GetElementByGuid(guid))
                   select Wrapper.Wrap(e);
        }

        public void PropagateChanges(ModelEntity.Element element)
        {
            Repo.Val.AdviseElementChange(element.EaObject.ElementID);
        }

        public virtual ModelEntity.Element Create(String name, ElementStereotype stereotype, ModelEntity.Package package)
        {
            var e = package.EaObject.Elements.AddNew(name, stereotype.Type.Name) as EA.Element;

            e.Stereotype = stereotype.Name;

            try
            {
                e.Update();
            }
            catch (COMException ce)
            {
                throw new ApplicationException(e.GetLastError(), ce);
            }

            package.EaObject.Elements.Refresh();

            return Wrapper.Wrap(e);
        }

        public virtual Option<ModelEntity.Element> Instanciate(ModelEntity.Element classifier, ModelEntity.Package package, IEnumerable<ElementStereotype> stereotypes)
        {
            return (from stype in classifier.GetStereotype(stereotypes)
                    from instanceType in stype.InstanceType
                    let e = Create("", instanceType, package)
                    select e)
                   .Select(e =>
                   {
                       e.EaObject.ClassifierID = classifier.Id;
                       e.EaObject.Update();
                       return e;
                   });
        }

        public virtual ModelEntity.Connector Connect(ModelEntity.Element source, ModelEntity.Element target, ConnectorStereotype stereotype)
        {
            var c = source.EaObject.Connectors.AddNew("", stereotype.Type.Name) as EA.Connector;

            c.Stereotype = stereotype.Name;
            c.SupplierID = target.Id;

            try
            {
                c.Update();
            }
            catch (COMException ce)
            {
                throw new ApplicationException(c.GetLastError(), ce);
            }

            SpecifyComposition(stereotype, c);
            SpecifyNavigateability(stereotype, c);

            source.EaObject.Connectors.Refresh();
            target.EaObject.Connectors.Refresh();

            return Wrapper.Wrap(c);
        }

        private static void SpecifyComposition(ConnectorStereotype stereotype, EA.Connector c)
        {
            stereotype.CompositionKind.Do(compositionKind =>
            {
                var end =
                    compositionKind.End == CompositionKind.CompositionEnd.Source ? c.ClientEnd.AsOption() :
                    compositionKind.End == CompositionKind.CompositionEnd.Target ? c.SupplierEnd.AsOption() :
                    Options.None<EA.ConnectorEnd>();
                end.Do(e =>
                {
                    e.Aggregation = (int)compositionKind.Type;
                    e.Update();
                });
            });
        }

        private static void SpecifyNavigateability(ConnectorStereotype stereotype, EA.Connector c)
        {
            var direction = stereotype.Direction.GetOrElse((stereotype.Type as ConnectorType).DefaultDirection);
            if (c.ClientEnd.Navigable != direction.SourceNavigateability.Name)
            {
                c.ClientEnd.Navigable = direction.SourceNavigateability.Name;
                c.ClientEnd.Update();
            }
            if (c.SupplierEnd.Navigable != direction.TargetNavigateability.Name)
            {
                c.SupplierEnd.Navigable = direction.TargetNavigateability.Name;
                c.SupplierEnd.Update();
            }
        }

        public Option<ModelEntity.Package> GetPackage(int id)
        {
            return from p in Options.Try(() => Repo.Val.GetPackageByID(id))
                   select Wrapper.Wrap(p);
        }

        public Option<ModelEntity.Package> GetPackage(string guid)
        {
            return from p in Options.Try(() => Repo.Val.GetPackageByGuid(guid))
                   select Wrapper.Wrap(p);
        }

        public ModelEntity.Package FindPackageContaining(ModelEntity.Element element)
        {
            return GetPackage(element.EaObject.PackageID).Value;
        }
    }
}
