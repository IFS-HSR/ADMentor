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
    class UpdateMetadataOfNewElementsCommand : ICommand<Func<EA.Element>, EntityModified>
    {
        private readonly ModelEntityRepository Repo;

        public UpdateMetadataOfNewElementsCommand(ModelEntityRepository repo)
        {
            Repo = repo;
        }

        public EntityModified Execute(Func<EA.Element> getElement)
        {
            return Repo.Wrapper.Wrap(getElement())
                .Match<AdEntity>()
                .Match(
                    element =>
                    {
                        element.CopyDataFromClassifier(Repo.GetElement);
                        return EntityModified.Modified;
                    },
                    () => EntityModified.NotModified);
        }

        public bool CanExecute(Func<EA.Element> getElement)
        {
            return true;
        }
    }
}
