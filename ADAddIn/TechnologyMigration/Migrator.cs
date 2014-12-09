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
    public class Migrator
    {
        public Migrator(MDGTechnology currentTechnology)
        {
            CurrentTechnology = currentTechnology;
        }

        public MDGTechnology CurrentTechnology { get; private set; }

        public bool CanMigrate(ModelEntity e)
        {
            return GetOutDatedModelId(e).IsDefined;
        }

        public void Migrate(ModelEntity e)
        {
            GetOutDatedModelId(e).Do(_ =>
            {
                AddMissingTags(e);
                UpdateModelId(e);
            });
        }

        private void AddMissingTags(ModelEntity entity)
        {
            var missingTags = from stype in CurrentTechnology.Stereotypes
                              where entity.Is(stype)
                              from tag in stype.TaggedValues
                              where !entity.Get(tag).IsDefined
                              select tag;

            missingTags.ForEach(missingTag =>
            {
                var defaultValue = (from defaultableType in missingTag.Type.TryCast<IDefaultableTaggedValueType>()
                                    from dv in defaultableType.DefaultValueAsString
                                    select dv);

                entity.Set(missingTag, defaultValue.GetOrElse(""));
            });
        }

        private void UpdateModelId(ModelEntity entity)
        {
            entity.Set(CurrentTechnology.ModelIdTag, CurrentTechnology.ModelId.ToString());
        }

        private Option<ModelId> GetOutDatedModelId(ModelEntity e)
        {
            return from raw in e.Get(CurrentTechnology.ModelIdTag)
                   from modelId in ModelId.Parse(raw)
                   where modelId.IsPredecessorOf(CurrentTechnology.ModelId)
                   select modelId;
        }
    }
}
