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

            var updateMetadataCommand = new UpdateMetadataOfNewElementsCommand(entityRepository);
            var updateStateOnAlternativesChanged = new UpdateProblemOccurrenceStateOnAlternativesChanged(entityRepository);
            var updateStateOnAlternativesAdded = new UpdateProblemOccurrenceStateOnAlternativesAdded(entityRepository);
            var updateStateOnRemoveAlternative = new UpdateProblemOccurrenceStateOnRemoveAlternative(entityRepository);
            var populateDependenciesCommand = new PopulateDependenciesCommand(
                entityRepository, new DependencySelectorForm(entityRepository));
            var instantiateProblemSpace = new InstantiateProblemSpaceCommand(entityRepository, new InstantiateSolutionForm());
            var exportProblemSpace = new ExportProblemSpaceCommand(
                entityRepository, new TailorPackageExportForm(), new XmlExporter.Factory(entityRepository, eaRepository), new SelectExportPathDialog());

            Register(new Menu(technology.Name,
                new MenuItem("Locate Option/Problem", new GoToClassifierCommand(eaRepository)),
                new MenuItem("Establish Dependencies from Problem Space", populateDependenciesCommand.AsMenuCommand()),
                new MenuItem("Tailor Problem Space", exportProblemSpace.AsMenuCommand()),
                new MenuItem("Create Solution from Problem Space", instantiateProblemSpace.AsMenuCommand()),
                new MenuItem("TEST", new TestFilterConfigurationCommand().AsMenuCommand())));

            OnEntityCreated.Add(updateMetadataCommand.AsEntityCreatedHandler());
            OnEntityCreated.Add(populateDependenciesCommand.AsEntityCreatedHandler());
            OnEntityCreated.Add(updateStateOnAlternativesAdded.AsOnEntityCreatedHandler());

            OnEntityModified.Add(updateStateOnAlternativesChanged.AsEntityModifiedHandler());

            OnDeleteEntity.Add(updateStateOnRemoveAlternative.AsOnDeleteEntityHandler());

            return Options.Some(entityWrapper);
        }
    }
}
