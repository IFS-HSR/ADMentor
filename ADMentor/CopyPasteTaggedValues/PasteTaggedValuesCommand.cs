using EAAddInBase;
using EAAddInBase.DataAccess;
using EAAddInBase.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADMentor.CopyPasteTaggedValues
{
    class PasteTaggedValuesCommand : ICommand<ModelEntity, Unit>
    {
        private readonly IReadableAtom<TaggedValuesClipboard> Clipboard;
        private readonly ModelEntityRepository Repo;

        public PasteTaggedValuesCommand(IReadableAtom<TaggedValuesClipboard> clipboard, ModelEntityRepository repo)
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
                Clipboard.Val.PasteInto(e);
                Repo.PropagateChanges(e);
            }
            else
            {
                selectedInCurrentDiagram.ForEach(selected =>
                {
                    Clipboard.Val.PasteInto(selected);
                    Repo.PropagateChanges(selected);
                });
            }

            return Unit.Instance;
        }

        public bool CanExecute(ModelEntity e)
        {
            return Clipboard.Val.CanPaste(e);
        }
    }
}
