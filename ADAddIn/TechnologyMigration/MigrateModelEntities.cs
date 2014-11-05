using AdAddIn.ADTechnology;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace AdAddIn.TechnologyMigration
{
    internal class MigrateModelEntities : ICommand<ModelEntity, Unit>
    {
        private readonly Migrator Migrator;
        private readonly MigrationCollector Collector;

        public MigrateModelEntities(Migrator migrator)
        {
            Migrator = migrator;
            Collector = new MigrationCollector(Migrator);
        }

        public Unit Execute(ModelEntity entity)
        {
            var entities = Collector.CollectCandidates(entity).Run();

            if (entities.IsEmpty())
            {
                MessageBox.Show("No entities for migration found");
            }
            else
            {
                var res = MessageBox.Show(String.Format("{0} entities for migration found", entities.Count()), "Model Migration", MessageBoxButtons.OKCancel);

                if (res == DialogResult.OK)
                {
                    entities.ForEach(e =>
                    {
                        Migrator.Migrate(e);
                    });

                    MessageBox.Show("Migration successful");
                }
            }

            return Unit.Instance;
        }

        public bool CanExecute(ModelEntity entity)
        {
            return true;
        }

        internal ICommand<Option<ModelEntity>, Unit> AsMenuCommand()
        {
            return this.Adapt((Option<ModelEntity> ci) => ci);
        }
    }
}
