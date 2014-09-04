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
            var packageRepository = new PackageRepository(eaRepository);
            var diagramRepository = new DiagramRepository(eaRepository);

            var updateMetadataCommand = new UpdateMetadataOfNewElementsCommand(elementRepository);
            var populateDependenciesCommand = new PopulateDependenciesCommand(
                elementRepository, diagramRepository, new DependencySelectorForm(elementRepository));
            var instantiateProblemSpace = new InstantiateProblemSpaceCommand(packageRepository, elementRepository, diagramRepository, new InstantiateSolutionForm());

            Register(new Menu(technology.Name,
                new MenuItem("Locate Option/Problem", new GoToClassifierCommand(elementRepository, eaRepository)),
                new MenuItem("Establish Dependencies from Problem Space", populateDependenciesCommand.AsMenuCommand()),
                new MenuItem("Tailor Problem Space", new ExportProblemSpaceCommand(elementRepository, packageRepository, new TailorSolutionForm()).AsMenuCommand()),
                new MenuItem("Create Solution from Problem Space", instantiateProblemSpace.AsMenuCommand())));

            OnElementCreated.Add(updateMetadataCommand);
            OnElementCreated.Add(populateDependenciesCommand.AsElementCreatedHandler());
        }
    }
}
