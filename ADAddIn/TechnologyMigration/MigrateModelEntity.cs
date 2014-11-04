using AdAddIn.ADTechnology;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.TechnologyMigration
{
    internal class MigrateModelEntity : ICommand<ModelEntity, Unit>
    {
        private readonly MDGTechnology Technology;

        public MigrateModelEntity(MDGTechnology technology)
        {
            Technology = technology;
        }

        public Unit Execute(ModelEntity e)
        {
            GetOutDatedModelId(e).Do(modelId =>
            {
                AddMissingTags(e);
                UpdateModelId(e);
            });

            return Unit.Instance;
        }

        private void AddMissingTags(ModelEntity entity)
        {
            var missingTags = from stype in Technology.Stereotypes
                              where entity.Is(stype)
                              from tag in stype.TaggedValues
                              where !entity.Get(tag).IsDefined
                              select tag;

            missingTags.ForEach(missingTag =>
            {
                var defaultValue = missingTag.Match<IDefaultableTaggedValueType<String>>().Match(
                    defaultableTag => defaultableTag.DefaultValue.GetOrElse(""),
                    () => "");

                entity.Set(missingTag, defaultValue);
            });
        }

        private void UpdateModelId(ModelEntity entity)
        {
            entity.Set(Technology.ModelIdTag, Technology.ModelId.ToString());
        }

        public bool CanExecute(ModelEntity e)
        {
            return GetOutDatedModelId(e).IsDefined;
        }

        private Option<ModelId> GetOutDatedModelId(ModelEntity e)
        {
            return from raw in e.Get(Technology.ModelIdTag)
                   from modelId in ModelId.Parse(raw)
                   where modelId.IsPredecessorOf(Technology.ModelId)
                   select modelId;
        }

        internal ICommand<Option<ModelEntity>, Unit> AsMenuCommand()
        {
            return this.Adapt((Option<ModelEntity> ci) => ci);
        }
    }
}
