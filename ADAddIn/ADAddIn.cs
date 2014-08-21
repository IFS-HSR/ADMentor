using AdAddIn.CopyMetadata;
using AdAddIn.DataAccess;
using AdAddIn.Navigation;
using AdAddIn.PopulateDependencies;
using AdAddIn.ADTechnology;
using EAAddInFramework;
using EAAddInFramework.MDGBuilder;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn
{
    public class ADAddIn : EAAddIn
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly MDGTechnology technology = ADTechnology.Technologies.AD;

        public override string AddInName
        {
            get { return technology.Name; }
        }

        public override Option<MDGTechnology> BootstrapTechnology()
        {
            return Options.Some(technology);
        }

        public override void bootstrap(IReadableAtom<EA.Repository> eaRepository)
        {
            var elementRepository = new ElementRepository(eaRepository);
            var diagreamRepository = new DiagramRepository(eaRepository);

            var updateMetadataCommand = new UpdateMetadataOfNewElementsCommand(elementRepository);
            var populateDependenciesCommand = new PopulateDependenciesCommand(
                elementRepository, diagreamRepository, new DependencySelectorForm(elementRepository));

            Register(new Menu(technology.Name,
                new MenuItem("Go to Classifier", new GoToClassifierCommand(elementRepository, eaRepository)),
                new MenuItem("Populate Dependencies", populateDependenciesCommand.AsMenuCommand())));

            OnElementCreated.Add(updateMetadataCommand);
            OnElementCreated.Add(populateDependenciesCommand.AsElementCreatedHandler());
        }
    }
}
