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
using AdAddIn.InstantiateProblemSpace;
using AdAddIn.ExportProblemSpace;
using EAAddInFramework.DataAccess;

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

        public override Option<IEntityWrapper> Bootstrap(IReadableAtom<EA.Repository> eaRepository)
        {
            var entityWrapper = new AdEntityWrapper();
            var entityRepository = new AdRepository(eaRepository, entityWrapper);
            var diagramRepository = new DiagramRepository(eaRepository);

            var updateMetadataCommand = new UpdateMetadataOfNewElementsCommand(entityRepository);
            var updateStatesCommand = new UpdateProblemOccurrenceStateCommand(entityRepository);
            var populateDependenciesCommand = new PopulateDependenciesCommand(
                entityRepository, diagramRepository, new DependencySelectorForm(entityRepository));
            var instantiateProblemSpace = new InstantiateProblemSpaceCommand(entityRepository, diagramRepository, new InstantiateSolutionForm());

            Register(new Menu(technology.Name,
                new MenuItem("Locate Option/Problem", new GoToClassifierCommand(eaRepository)),
                new MenuItem("Establish Dependencies from Problem Space", populateDependenciesCommand.AsMenuCommand()),
                new MenuItem("Tailor Problem Space", new ExportProblemSpaceCommand(entityRepository, new TailorPackageExportForm()).AsMenuCommand()),
                new MenuItem("Create Solution from Problem Space", instantiateProblemSpace.AsMenuCommand())));

            OnElementCreated.Add(updateMetadataCommand.AsElementCreatedHandler());
            OnElementCreated.Add(populateDependenciesCommand.AsElementCreatedHandler());

            OnElementModified.Add(updateStatesCommand.AsElementModifiedHandler());

            return Options.Some(entityWrapper);
        }
    }
}
