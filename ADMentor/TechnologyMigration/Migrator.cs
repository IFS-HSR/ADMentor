using EAAddInBase;
using EAAddInBase.DataAccess;
using EAAddInBase.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace ADMentor.TechnologyMigration
{
    public class Migrator : ICommand<ModelEntity, Unit>
    {
        public Migrator(MDGTechnology currentTechnology)
        {
            CurrentTechnology = currentTechnology;
        }

        public MDGTechnology CurrentTechnology { get; private set; }

        public bool CanExecute(ModelEntity e)
        {
            return GetOutDatedModelId(e).IsDefined;
        }

        public Unit Execute(ModelEntity e)
        {
            GetOutDatedModelId(e).Do(_ =>
            {
                AddMissingTags(e);
                UpdateModelId(e);
            });

            return Unit.Instance;
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

        public ICommand<ModelEntity, Option<ValidationMessage>> GetValidator()
        {
            return Command.Create<ModelEntity, Option<ValidationMessage>>(e => 
                this.CanExecute(e).Then(() => ValidationMessage.Warning("Model version not up to date, migration required")));
        }
    }
}
