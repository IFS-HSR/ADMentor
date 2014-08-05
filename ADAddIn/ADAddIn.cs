using AdAddIn.Navigation;
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
            Register(new Menu(technology.Name,
                new MenuItem("Foo", Command.Create((Option<ContextItem> arg) =>
                {
                    System.Windows.Forms.MessageBox.Show(String.Format("{0}", AdAddIn.Properties.Settings.Default.CdarUrl));
                    return Unit.Instance;
                })),
                new MenuItem("Go to Classifier", new GoToClassifierCommand(repository))));
            Register(new Components.PopulateDependenciesOnNewOccurrences());
            //Properties.Settings.CdarUrl = "Some(www.example.com)";
            //Register(new DecisionDetailViewComponent(new DecisionDetailView(), decisions, alternatives, issues));
        }
    }
}
