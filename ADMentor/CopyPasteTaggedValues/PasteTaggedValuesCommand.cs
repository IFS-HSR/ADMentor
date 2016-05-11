using EAAddInBase;
using EAAddInBase.DataAccess;
using EAAddInBase.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMentor.CopyPasteTaggedValues
{
    class PasteTaggedValuesCommand : ICommand<ModelEntity, Unit>
    {
        private readonly IReadableAtom<IImmutableDictionary<string, string>> Clipboard;
        private readonly ModelEntityRepository Repo;

        public PasteTaggedValuesCommand(IReadableAtom<IImmutableDictionary<String, String>> clipboard, ModelEntityRepository repo)
        {
            Clipboard = clipboard;
            Repo = repo;
        }

        public Unit Execute(ModelEntity e)
        {
            var selectedInCurrentDiagram = Repo.GetCurrentDiagram()
                .SelectMany(currentDiagram => currentDiagram.SelectedEntities(Repo))
                .ToList();

            if (selectedInCurrentDiagram.IsEmpty())
            {
                PasteInto(e);
            }
            else
            {
                selectedInCurrentDiagram.ForEach(selected =>
                {
                    PasteInto(selected);
                });
            }

            Repo.PropagateChanges(e);

            return Unit.Instance;
        }

        private void PasteInto(ModelEntity e)
        {
            Clipboard.Val.ForEach(tv =>
            {
                e.Set(tv.Key, tv.Value);
            });
        }

        public bool CanExecute(ModelEntity e)
        {
            return !Clipboard.Val.IsEmpty() && !(e is ModelEntity.Diagram);
        }
    }
}
