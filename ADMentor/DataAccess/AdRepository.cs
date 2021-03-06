﻿using EAAddInBase.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace ADMentor.DataAccess
{
    public class AdRepository : ModelEntityRepository
    {
        public AdRepository(IReadableAtom<EA.Repository> repo, IEntityWrapper wrapper) : base(repo, wrapper) { }

        public override Option<ModelEntity.Element> Instantiate(ModelEntity.Element classifier, ModelEntity.Package package, IEnumerable<EAAddInBase.MDGBuilder.ElementStereotype> stereotypes)
        {
            return base.Instantiate(classifier, package, stereotypes).Select(instance =>
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
