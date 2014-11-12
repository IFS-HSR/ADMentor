using AdAddIn.DataAccess;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.ExportToADRepo
{
    public class ExportToADRepoCommand : ICommand<ModelEntity.Package, Unit>
    {
        private readonly ADRepoClient client;

        public ExportToADRepoCommand(AdRepository repo)
        {
            client = new ADRepoClient(new Uri("http://localhost:9000/"), repo);
        }

        public Unit Execute(ModelEntity.Package package)
        {
            var entities = from p in package.SubPackages()
                           from e in p.Elements()
                           from entity in e.Match<AdEntity>()
                           select entity;

            entities.ForEach(client.ExportEntity);

            return Unit.Instance;
        }

        public bool CanExecute(ModelEntity.Package package)
        {
            return true;
        }

        public ICommand<Option<ModelEntity>, object> AsMenuCommand()
        {
            return this.Adapt((Option<ModelEntity> contextItem) =>
            {
                return from ci in contextItem
                       from package in ci.Match<ModelEntity.Package>()
                       select package;
            });
        }
    }
}
