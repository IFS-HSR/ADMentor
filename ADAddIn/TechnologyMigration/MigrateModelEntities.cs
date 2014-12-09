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

        public MigrateModelEntities(Migrator migrator)
        {
            Migrator = migrator;
        }

        public Unit Execute(ModelEntity entity)
        {
            var entities = CollectCandidates(entity).Run();

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
                        Migrator.Execute(e);
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

        private IEnumerable<ModelEntity> CollectCandidates(ModelEntity rootEntity)
        {
            return from candidate in rootEntity.Match<ModelEntity, IEnumerable<ModelEntity>>()
                        .Case<ModelEntity.Package>(p => CollectChildren(p))
                        .Case<ModelEntity.Element>(e => e.Connectors.Concat(new[] { rootEntity }))
                        .Default(entity => new[] { entity })
                   where Migrator.CanExecute(candidate)
                   select candidate;
        }

        private IEnumerable<ModelEntity> CollectChildren(ModelEntity.Package package)
        {
            var packages = from p in package.SubPackages
                           select p;

            var elements = from p in package.SubPackages
                           from e in p.Elements
                           select e;

            var connectors = from e in elements
                             from c in e.Connectors
                             select c;

            return packages
                .Concat<ModelEntity>(elements)
                .Concat<ModelEntity>(connectors)
                .Distinct();
        }
    }
}
