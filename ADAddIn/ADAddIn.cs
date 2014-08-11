using AdAddIn.Navigation;
using AdAddIn.PopulateDependencies;
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

        public override Option<MDGTechnology> bootstrapTechnology()
        {
            return technology.AsOption();
        }

        public override void bootstrap(IReadableAtom<EA.Repository> repository)
        {
            var populateDependenciesCommand = new PopulateDependenciesCommand(
                repository, new MissingADDependenciesFinder(repository), new DependencySelectorForm());

            Register(new Menu(technology.Name,
                new MenuItem("Go to Classifier", new GoToClassifierCommand(repository)),
                new MenuItem("Populate Dependencies", populateDependenciesCommand.AsMenuCommand())));

            OnElementCreated.Add(new CopyMetadataOfNewSolutionItemsCommand(repository));
            OnElementCreated.Add(populateDependenciesCommand.AsElementCreatedHandler());
        }
    }
}
