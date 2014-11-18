using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdAddIn.TechnologyMigration
{
    public class MigrationCollector
    {
        private readonly Migrator Migrator;

        public MigrationCollector(Migrator migrator)
        {
            this.Migrator = migrator;
        }

        public IEnumerable<ModelEntity> CollectCandidates(ModelEntity rootEntity)
        {
            return from candidate in rootEntity.Match(
                        (ModelEntity.Package p) => CollectChildren(p),
                        (ModelEntity.Element e) => e.Connectors.Concat(new[] { rootEntity }),
                        () => new ModelEntity[] { rootEntity })
                   where Migrator.CanMigrate(candidate)
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
