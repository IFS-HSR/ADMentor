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

            return Options.Try(() => new Uri(adRepoHost)).Fold(
                uri =>
                {
                    using (var client = new ADRepoClient(uri, repository))
                    {
                        client.ExportPackage(package);
                    }
                    return Unit.Instance;
                },
                () =>
                {
                    if (adRepoHost != "")
                    {
                        MessageBox.Show(String.Format("Invalid URL \"{0}\", please try again.", adRepoHost));
                        return Execute(package);
                    }
                    else
                    {
                        return Unit.Instance;
                    }
                });
        }

        public bool CanExecute(ModelEntity.Package package)
        {
            return true;
        }
    }
}
