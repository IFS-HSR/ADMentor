using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEntityWrapper Wrapper { get; private set; }

        public Option<ModelEntity.Element> GetElement(int id)
        {
            return from e in Repo.Val.GetElementByID(id).AsOption()
                   select Wrapper.Wrap(e);
        }

        public Option<ModelEntity.Element> GetElement(string guid)
        {
            return from e in Repo.Val.GetElementByGuid(guid).AsOption()
                   select Wrapper.Wrap(e);
        }

        public void PropagateChanges(ModelEntity.Element element)
        {
            Repo.Val.AdviseElementChange(element.EaObject.ElementID);
        }

        public virtual Option<ModelEntity.Element> Instanciate(ModelEntity.Element classifier, ModelEntity.Package package, IEnumerable<ElementStereotype> stereotypes)
        {
            return from stype in classifier.GetStereotype(stereotypes)
                   from instanceType in stype.InstanceType
                   from instance in stype.Instanciate(classifier.EaObject, package.EaObject)
                   select Wrapper.Wrap(instance);
        }

        public Option<ModelEntity.Package> GetPackage(int id)
        {
            return from p in Repo.Val.GetPackageByID(id).AsOption()
                   select Wrapper.Wrap(p);
        }

        public Option<ModelEntity.Package> GetPackage(string guid)
        {
            return from p in Repo.Val.GetPackageByGuid(guid).AsOption()
                   select Wrapper.Wrap(p);
        }
    }
}
