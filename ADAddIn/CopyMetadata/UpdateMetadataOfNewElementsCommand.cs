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

        public ICommand<EA.Element, EntityModified> AsElementCreatedHandler()
        {
            return this.Adapt((EA.Element element) =>
            {
                return Repo.Wrapper.Wrap(element).Match<AdEntity>();
            });
        }
    }
}
