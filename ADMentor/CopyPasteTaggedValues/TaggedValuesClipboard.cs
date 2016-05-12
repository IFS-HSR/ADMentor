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
    public class TaggedValuesClipboard
    {
        private readonly Option<ModelEntity> SourceEntity;
        private readonly IImmutableSet<String> CopiedTagNames;

        public TaggedValuesClipboard()
        {
            SourceEntity = Options.None<ModelEntity>();
            CopiedTagNames = ImmutableHashSet.Create<String>();
        }

        public TaggedValuesClipboard(ModelEntity sourceEntity, IImmutableSet<String> copiedTagNames)
        {
            SourceEntity = Options.Some(sourceEntity);
            CopiedTagNames = copiedTagNames;
        }

        public bool PasteInto(ModelEntity targetEntity)
        {
            return SourceEntity
                .Select(source =>
                    CopiedTagNames.All(tagName => targetEntity.InsertTaggedValueFrom(source, tagName)))
                .GetOrElse(false);
        }

        public bool CanPaste()
        {
            return SourceEntity.IsDefined
                && !CopiedTagNames.IsEmpty();
        }

        public bool CanPaste(ModelEntity targetEntity)
        {
            var targetTags = targetEntity.TaggedValues.Select(p => p.Key).ToList();

            return CanPaste()
                && CopiedTagNames.All(tag => targetTags.Contains(tag));
        }
    }
}
