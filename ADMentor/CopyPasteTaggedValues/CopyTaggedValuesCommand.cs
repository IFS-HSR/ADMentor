using EAAddInBase;
using EAAddInBase.DataAccess;
using EAAddInBase.MDGBuilder;
using EAAddInBase.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMentor.CopyPasteTaggedValues
{
    class CopyTaggedValuesCommand : ICommand<ModelEntity, Unit>
    {
        private readonly IWriteableAtom<TaggedValuesClipboard> Clipboard;
        private readonly SelectTaggedValuesForm Form;

        public CopyTaggedValuesCommand(IWriteableAtom<TaggedValuesClipboard> clipboard)
        {
            Clipboard = clipboard;
            Form = new SelectTaggedValuesForm();
        }

        public Unit Execute(ModelEntity e)
        {
            var excludedTypes = new[] { TaggedValueTypes.Const("").TypeName };

            var tvs = ImmutableDictionary.CreateRange(
                from tv in e.TaggedValues
                join proto in ADTechnology.Technologies.AD.TaggedValues on tv.Key equals proto.Name
                where !proto.Type.TypeName.In(excludedTypes)
                select tv);

            Form.GetSelected(tvs).Do(selectedTVs =>
            {
                var selectedTags = selectedTVs.Select(p => p.Key).ToImmutableHashSet();
                Clipboard.Exchange(new TaggedValuesClipboard(e, selectedTags), GetType());
            });

            return Unit.Instance;
        }

        public bool CanExecute(ModelEntity e)
        {
            return !e.TaggedValues.IsEmpty;
        }
    }
}
