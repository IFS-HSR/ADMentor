using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using EAAddInFramework;
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
        private readonly ElementRepository Repo;

        public UpdateMetadataOfNewElementsCommand(ElementRepository repo)
        {
            Repo = repo;
        }

        public EntityModified Execute(Func<EA.Element> getElement)
        {
            var element = getElement();

            return Repo.UpdateMetadata(element);
        }

        public bool CanExecute(Func<EA.Element> getElement)
        {
            return true;
        }
    }
}
