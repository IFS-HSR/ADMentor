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
            Clipboard.Val.ForEach(tv =>
            {
                e.Set(tv.Key, tv.Value);
            });

            Repo.PropagateChanges(e);

            return Unit.Instance;
        }

        public bool CanExecute(ModelEntity e)
        {
            return !Clipboard.Val.IsEmpty() && !(e is ModelEntity.Diagram);
        }
    }
}
