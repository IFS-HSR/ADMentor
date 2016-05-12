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
    public class PasteTaggedValuesIntoChildrenCommand : ICommand<ModelEntity.Package, Unit>
    {
        private readonly IReadableAtom<TaggedValuesClipboard> Clipboard;
        private readonly ModelEntityRepository Repo;
        private readonly SelectDescendantsForm Form;

        public PasteTaggedValuesIntoChildrenCommand(IReadableAtom<TaggedValuesClipboard> clipboard, ModelEntityRepository repo)
        {
            Clipboard = clipboard;
            Repo = repo;
            Form = new SelectDescendantsForm();
        }

        public Unit Execute(ModelEntity.Package p)
        {
            var content = PackageTreeNode(p).Value;

            Form.Select(content).Do(selectedEntities =>
            {
                selectedEntities.ForEach(e => {
                    Clipboard.Val.PasteInto(e);
                    Repo.PropagateChanges(e);
                });
            });

            return Unit.Instance;
        }

        private Option<PackageTree> PackageTreeNode(ModelEntity entity)
        {
            var selectable = Clipboard.Val.CanPaste(entity);

            return entity.Match<ModelEntity, PackageTree>()
                .Case<ModelEntity.Package>(p => new PackageTree(p, p.Entities.SelectMany(PackageTreeNode).ToImmutableList(), selectable))
                .Case<ModelEntity.Element>(e => new PackageTree(e, selectable: selectable))
                .Case<ModelEntity.Connector>(c => new PackageTree(c, selectable: selectable))
                .GetOrNone();
        }

        public bool CanExecute(ModelEntity.Package p)
        {
            return Clipboard.Val.CanPaste();
        }
    }
}
