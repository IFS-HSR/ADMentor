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
            GetModelId(e).Do(modelId =>
            {
                AddRefinementLevelTag(e);
                UpdateModelId(e);
            });

            return Unit.Instance;
        }

        private void AddRefinementLevelTag(ModelEntity entity)
        {
            var stereotypesWithRefinementLevel =
                from stype in Technology.Stereotypes
                where stype.TaggedValues.Contains(Common.RefinementLevel)
                select stype;

            Func<ModelEntity.Element, Unit> updateElement = element =>
            {
                if (stereotypesWithRefinementLevel.Any(stype => element.Is(stype)))
                {
                    if (!element.Get(Common.RefinementLevel).IsDefined)
                    {
                        element.Set(Common.RefinementLevel, "");
                    }
                }
                return Unit.Instance;
            };

            entity.Match(
                (ModelEntity.Element element) => updateElement(element),
                (ModelEntity.Package package) => updateElement(package.Element()),
                () => Unit.Instance);
        }

        private void UpdateModelId(ModelEntity entity)
        {

            Func<ModelEntity.Element, Unit> updateElement = element =>
            {
                element.Set(Technology.ModelIdTag, Technology.ModelId.ToString());
                return Unit.Instance;
            };

            entity.Match(
                (ModelEntity.Element element) => updateElement(element),
                (ModelEntity.Package package) => updateElement(package.Element()),
                () => Unit.Instance);
        }

        public bool CanExecute(ModelEntity e)
        {
            return GetModelId(e).Match(
                mid => mid.IsPredecessorOf(Technology.ModelId),
                () => false);
        }

        private Option<ModelId> GetModelId(ModelEntity e)
        {
            return from raw in e.Get(Technology.ModelIdTag)
                   from modelId in ModelId.Parse(raw)
                   select modelId;
        }

        internal ICommand<Option<ModelEntity>, Unit> AsMenuCommand()
        {
            return this.Adapt((Option<ModelEntity> ci) => ci);
        }
    }
}
