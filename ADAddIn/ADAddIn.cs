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
using AdAddIn.TechnologyMigration;
using AdAddIn.Analysis;
using AdAddIn.Validation;

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

        public override Tuple<Option<IEntityWrapper>, IEnumerable<ValidationRule>> Bootstrap(IReadableAtom<EA.Repository> eaRepository)
        {
            var entityWrapper = new AdEntityWrapper();
            var entityRepository = new AdRepository(eaRepository, entityWrapper);
            var migrator = new Migrator(technology);

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
                new MenuItem("Tailor Problem Space", exportProblemSpace.ToMenuHandler()),
                new MenuItem("Create Solution Space from Problem Space", instantiateProblemSpace.ToMenuHandler()),
                new MenuItem("Locate Option/Problem", new GoToClassifierCommand(eaRepository)),
                new MenuItem("Establish Dependencies from Problem Space", populateDependenciesCommand.ToMenuHandler()),
                new MenuItem("Migrate Element(s) to Current Model Version", new MigrateModelEntities(migrator).ToMenuHandler()),
                new MenuItem("Package Metrics", new AnalysePackageCommand(entityRepository, new DisplayMetricsForm()).ToMenuHandler()),
                //new MenuItem("TEST", new TestFilterConfigurationCommand().AsMenuCommand()),
                new MenuItem("Export to AD Repo", new ExportToADRepo.ExportToADRepoCommand(entityRepository).ToMenuHandler())));

            OnEntityCreated.Add(updateMetadataCommand.ToEntityCreatedHandler());
            OnEntityCreated.Add(populateDependenciesCommand.AsEntityCreatedHandler());
            OnEntityCreated.Add(updateStateOnAlternativesAdded.ToEntityCreatedHandler());

            OnEntityModified.Add(updateStateOnAlternativesChanged.ToEntityModifiedHandler());

            OnDeleteEntity.Add(updateStateOnRemoveAlternative.AsOnDeleteEntityHandler());

            var rules = new []{
                new ValidationRule(technology.Name, new ValidateProblemOptionCompositionCommand(entityRepository).ToValidator())
            };

            return Tuple.Create(
                Options.Some(entityWrapper as IEntityWrapper), 
                rules.AsEnumerable());
        }
    }
}
