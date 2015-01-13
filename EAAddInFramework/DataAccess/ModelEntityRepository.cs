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

        public object GetEntity(string guid)
        {
            return (GetElement(guid) as Option<ModelEntity>)
                .OrElse(() => GetConnector(guid))
                .OrElse(() => GetDiagram(guid))
                .OrElse(() => GetPackage(guid));
        }

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

        public Option<ModelEntity.Connector> GetConnector(string guid)
        {
            return from c in Options.Try(() => Repo.Val.GetConnectorByGuid(guid))
                   select Wrapper.Wrap(c);
        }

        public Option<ModelEntity.Connector> GetConnector(int id)
        {
            return from c in Options.Try(() => Repo.Val.GetConnectorByID(id))
                   select Wrapper.Wrap(c);
        }

        public virtual ModelEntity.Connector Connect(ModelEntity.Element source, ModelEntity.Element target, ConnectorStereotype stereotype)
        {
            if (stereotype.Type == ConnectorType.Association && (source.Type.Equals(ElementType.Package.Name) || target.Type.Equals(ElementType.Package.Name)))
            {
                // seems strange but it's true; EA crashes if you try to create such a connection! Thus, we stop before it's getting really ugly.
                throw new ApplicationException(String.Format("Cannot create connection {0} - {1} - {2}", source, stereotype.Name, target));
            }

            var c = source.EaObject.Connectors.AddNew("", stereotype.Type.Name) as EA.Connector;

            c.Stereotype = stereotype.Name;
            c.SupplierID = target.Id;

            try
            {
                c.Update();
            }
            catch (Exception e)
            {
                throw new ApplicationException(c.GetLastError(), e);
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

        public ModelEntity.Package CreatePackage(String name, Option<ModelEntity.Package> parent, Option<PackageStereotype> stereotype)
        {
            var collection = parent.Select(pa => pa.EaObject.Packages).GetOrElse(Repo.Val.Models);

            var p = collection.AddNew(name, "") as EA.Package;

            try
            {
                p.Update();
            }
            catch (COMException ce)
            {
                throw new ApplicationException(p.GetLastError(), ce);
            }

            (from e in p.Element.AsOption()
             from stype in stereotype
             select Tuple.Create(e, stype)).ForEach((e, stype) =>
             {
                 e.Stereotype = stype.Name;
                 e.Update();
             });

            collection.Refresh();

            return Wrapper.Wrap(p);
        }

        public Option<ModelEntity.Diagram> GetDiagram(int id)
        {
            return from d in Options.Try(() => Repo.Val.GetDiagramByID(id))
                   select Wrapper.Wrap(d);
        }

        public Option<ModelEntity.Diagram> GetDiagram(string guid)
        {
            return from d in Options.Try(() => Repo.Val.GetDiagramByGuid(guid) as EA.Diagram)
                   select Wrapper.Wrap(d);
        }

        public Option<ModelEntity.Diagram> GetCurrentDiagram()
        {
            return Repo.Val.GetCurrentDiagram()
                .AsOption()
                .Select(Wrapper.Wrap);
        }

        public void Reload(ModelEntity.Diagram diagram)
        {
            Repo.Val.ReloadDiagram(diagram.Id);
        }

        public void Save(ModelEntity.Diagram diagram)
        {
            Repo.Val.SaveDiagram(diagram.Id);
        }

        public void SaveAllDiagrams()
        {
            Repo.Val.SaveAllDiagrams();
        }

        public ModelEntity.Diagram Create(string name, Diagram diagramType, ModelEntity.Package package, String technologyId)
        {
            var diagram = package.EaObject.Diagrams.AddNew(name, diagramType.Type.Name) as EA.Diagram;
            diagram.StyleEx = String.Format("MDGDgm={0}::{1};", technologyId, diagramType.Name);
            diagram.Update();
            package.EaObject.Diagrams.Refresh();
            return Wrapper.Wrap(diagram);
        }

        public IEnumerable<ModelEntity.Package> Packages
        {
            get
            {
                return Repo.Val.Models.Cast<EA.Package>().Select(p => Wrapper.Wrap(p)).Run();
            }
        }

        public IEnumerable<ModelEntity.Package> AllPackages
        {
            get
            {
                return Packages.SelectMany(p => p.SubPackages);
            }
        }
    }
}
