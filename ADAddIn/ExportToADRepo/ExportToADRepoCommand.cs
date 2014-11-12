using AdAddIn.DataAccess;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace AdAddIn.ExportToADRepo
{
    public class ExportToADRepoCommand : ICommand<ModelEntity.Package, Unit>
    {
        private readonly AdRepository repository;

        public ExportToADRepoCommand(AdRepository repo)
        {
            repository = repo;
        }

        public Unit Execute(ModelEntity.Package package)
        {
            var adRepoHost = Microsoft.VisualBasic.Interaction.InputBox("ADRepo URL", "Export to ADRepo", "http://localhost:9000/");
            var client = new ADRepoClient(new Uri(adRepoHost), repository);
            client.ExportPackage(package);
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
