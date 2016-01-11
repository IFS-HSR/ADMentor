using ADMentor.DataAccess;
using EAAddInBase;
using EAAddInBase.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EAAddInBase.Utils;

namespace ADMentor.ADRepoConnector
{
    sealed class ExportToADRepoCommand : ICommand<ModelEntity.Package, Unit>
    {
        private readonly AdRepository repository;
        private readonly CommitForm form;
        private readonly EaToAdRepoConverter converter;

        public ExportToADRepoCommand(AdRepository repo)
        {
            repository = repo;
            form = new CommitForm();
            converter = new EaToAdRepoConverter(repo);
        }

        public Unit Execute(ModelEntity.Package package)
        {
            var adRepoHost = Microsoft.VisualBasic.Interaction.InputBox("ADRepo URL", "Export to ADRepo", "http://localhost:8080/");

            var model = converter.ToModel(package);

            return Options.Try(() => new Uri(adRepoHost)).Fold(
                uri =>
                {
                    using (var client = new ADRepoClient(uri))
                    {
                        var tags = client.GetModels().Result;

                        form.AskForCommit(model, tags).Do(commit =>
                        {
                            var id = client.CommitModel(commit).Result;
                            MessageBox.Show(String.Format("Model {0} successfully commited with id {1}", model.root.Join("/"), id));
                        });
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
