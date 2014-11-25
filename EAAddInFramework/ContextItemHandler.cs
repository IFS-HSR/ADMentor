using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace EAAddInFramework
{
    class ContextItemHandler
    {
        private readonly IReadableAtom<EA.Repository> repository;

        private readonly IReadableAtom<IEntityWrapper> entityWrapper;

        private readonly Atom<Option<ModelEntity>> contextItem;

        public ContextItemHandler(Atom<Option<ModelEntity>> ci, IReadableAtom<EA.Repository> repo, IReadableAtom<IEntityWrapper> wrapper)
        {
            repository = repo;
            entityWrapper = wrapper;
            contextItem = ci;
        }

        private void UpdateContextItem(Option<object> candidate)
        {
            var entity = from ci in candidate
                         select entityWrapper.Val.Wrap(ci as dynamic) as ModelEntity;
            contextItem.Exchange(entity, GetType());
        }

        public void ContextItemChanged(string guid, EA.ObjectType ot)
        {
            switch (ot)
            {
                case EA.ObjectType.otElement:
                    UpdateContextItem(repository.Val.GetElementByGuid(guid).AsOption());
                    break;
                case EA.ObjectType.otConnector:
                    UpdateContextItem(repository.Val.GetConnectorByGuid(guid).AsOption());
                    break;
                case EA.ObjectType.otDiagram:
                    UpdateContextItem(repository.Val.GetDiagramByGuid(guid).AsOption());
                    break;
                case EA.ObjectType.otPackage:
                    UpdateContextItem(repository.Val.GetPackageByGuid(guid).AsOption());
                    break;
                default:
                    UpdateContextItem(Options.None<object>());
                    break;
            }
        }

        public void MenuLocationChanged(string menuLocation)
        {
            if (menuLocation == "MainMenu")
            {
                // there must not be a context item when the main menu is open
                UpdateContextItem(Options.None<object>());
            }
            if (repository.Val.GetTreeSelectedItemType() == EA.ObjectType.otPackage)
            {
                // selecting a model in the project explorer does not trigger a context item changed event
                var selectedPkg = repository.Val.GetTreeSelectedObject() as EA.Package;
                if (selectedPkg.IsModel)
                {
                    UpdateContextItem(Options.Some(selectedPkg));
                }
            }
        }
    }
}
