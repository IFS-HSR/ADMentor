using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.DataAccess
{
    public class AdRepository : ModelEntityRepository
    {
        public AdRepository(IReadableAtom<EA.Repository> repo, IEntityWrapper wrapper) : base(repo, wrapper) { }

        public override Option<ModelEntity.Element> Instanciate(ModelEntity.Element classifier, ModelEntity.Package package, IEnumerable<EAAddInFramework.MDGBuilder.ElementStereotype> stereotypes)
        {
            return base.Instanciate(classifier, package, stereotypes).Select(instance =>
            {
                instance.TryCast<AdEntity>().Do(adInstance =>
                {
                    adInstance.CopyDataFromClassifier(GetElement);
                });
                return instance;
            });
        }
    }
}
