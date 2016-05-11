using ADMentor.CopyMetadata;
using ADMentor.DataAccess;
using ADMentor.Navigation;
using ADMentor.PopulateDependencies;
using ADMentor.ADTechnology;
using EAAddInBase;
using EAAddInBase.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;
using ADMentor.InstantiateProblemSpace;
using ADMentor.ExportProblemSpace;
using EAAddInBase.DataAccess;
using ADMentor.TechnologyMigration;
using ADMentor.Analysis;
using ADMentor.Validation;
using ADMentor.UsabilityShortCuts;
using System.Collections.Immutable;
using ADMentor.CopyPasteTaggedValues;

namespace ADMentor
{
    public sealed class ADMentorAddIn : EAAddIn
    {
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

            var tvClipboard = new Atom<IImmutableDictionary<String, String>>(ImmutableDictionary.Create<String, String>());

            var updateMetadataCommand = new UpdateMetadataOfNewElementsCommand(entityRepository);
            var updateStateOnAlternativesChanged = new UpdateProblemOccurrenceStateOnAlternativesChanged(entityRepository);
            var updateStateOnAlternativesAdded = new UpdateProblemOccurrenceStateOnAlternativesAdded(entityRepository);
            var updateStateOnRemoveAlternative = new UpdateProblemOccurrenceStateOnAlternativeRemoved(entityRepository);
            var populateDependenciesCommand = new PopulateDependenciesCommand(
                entityRepository, new DependencySelectorForm(entityRepository));
            var instantiateProblemSpace = new InstantiateProblemSpaceCommand(entityRepository, new InstantiateSolutionForm());
            var exportProblemSpace = new ExportProblemSpaceCommand(
                entityRepository, new TailorPackageExportForm(), new XmlExporter.Factory(entityRepository, eaRepository), new SelectExportPathDialog());

            Register(new Menu(AddInName,
                new MenuItem("Copy Tagged Values",
                    new CopyTaggedValuesCommand(tvClipboard).ToMenuHandler()),
                new MenuItem("Paste Tagged Values",
                    new PasteTaggedValuesCommand(tvClipboard, entityRepository).ToMenuHandler()),
                new Separator(),
                new MenuItem("Choose Selected and Neglect not chosen Alternatives",
                    new ChooseOptionOccurrenceCommand<Unit>(entityRepository, updateStateOnAlternativesChanged).ToMenuHandler()),
                new MenuItem("Neglect all Alternatives",
                    new NeglectAllOptionsCommand(entityRepository).ToMenuHandler()),
                new Separator(),
                new MenuItem("Tailor Problem Space",
                    exportProblemSpace.ToMenuHandler()),
                new MenuItem("Create Solution Space from Problem Space",
                    instantiateProblemSpace.ToMenuHandler()),
                new MenuItem("Locate Option/Problem",
                    new GoToClassifierCommand(entityRepository).ToMenuHandler()),
                new MenuItem("Establish Dependencies from Problem Space",
                    populateDependenciesCommand.ToMenuHandler()),
                new MenuItem("Migrate Element(s) to Current Model Version",
                    new MigrateModelEntitiesCommand(migrator).ToMenuHandler()),
                new Separator(),
                new MenuItem("Package Metrics",
                    new AnalysePackageCommand(entityRepository, new DisplayMetricsForm()).ToMenuHandler())));

            OnEntityCreated.Add(updateMetadataCommand.ToEntityCreatedHandler());
            OnEntityCreated.Add(updateStateOnAlternativesAdded.ToEntityCreatedHandler());

            OnEntityModified.Add(updateStateOnAlternativesChanged.ToEntityModifiedHandler());

            OnDeleteEntity.Add(updateStateOnRemoveAlternative.AsOnDeleteEntityHandler());

            var rules = new[]{
                ValidationRule.FromCommand(AddInName, new ValidateProblemOptionCompositionCommand(entityRepository)),
                ValidationRule.FromCommand(AddInName, new ValidateProblemOccurrenceStateCommand(entityRepository)),
                ValidationRule.FromCommand(AddInName, new ValidateConflictingOptionsCommand(entityRepository)),
                ValidationRule.FromCommand(AddInName, new MultipleProblemsAddressedByAnOptionCommand(entityRepository)),
                ValidationRule.FromCommand(AddInName, migrator.GetValidator()),
                new ElementNotUsedDiagramRule(AddInName, entityRepository)
            };

            return Tuple.Create(
                Options.Some(entityWrapper as IEntityWrapper),
                rules.AsEnumerable());
        }
    }
}
