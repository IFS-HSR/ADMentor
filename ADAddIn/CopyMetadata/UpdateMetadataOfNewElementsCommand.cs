using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.CopyMetadata
{
    class UpdateMetadataOfNewElementsCommand : ICommand<AdEntity, EntityModified>
    {
        private readonly ModelEntityRepository Repo;

        public UpdateMetadataOfNewElementsCommand(ModelEntityRepository repo)
        {
            Repo = repo;
        }

        public EntityModified Execute(AdEntity entity)
        {
            entity.CopyDataFromClassifier(Repo.GetElement);
            return EntityModified.Modified;
        }

        public bool CanExecute(AdEntity _)
        {
            return true;
        }

        public ICommand<ModelEntity, EntityModified> AsEntityCreatedHandler()
        {
            return this.Adapt((ModelEntity entity) =>
            {
                return entity.Match<AdEntity>();
            });
        }
    }
}
