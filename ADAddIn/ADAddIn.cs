using AdAddIn.DataAccess;
using AdAddIn.DataAccess.ADModel;
using AdAddIn.DecisionDetails;
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
            var decisions = new ADDecisionRepository(repository);
            var alternatives = new ADAlternativeRepository(repository);
            var issues = new ADIssueRepository(repository);

            Register(new Menu(technology.Name,
                new Menu("Sub1", new DummyMenu("Item1"), new DummyMenu("Item2")),
                new Menu("Sub2", new DummyMenu("Item3"))));
            //Properties.Settings.CdarUrl = "Some(www.example.com)";
            //Register(new DecisionDetailViewComponent(new DecisionDetailView(), decisions, alternatives, issues));
        }
    }

    public class DummyMenu : MenuItem
    {
        public DummyMenu(String name) : base(name) { }

        public override void OnClick(Option<ContextItem> contextItem)
        {
            //System.Windows.Forms.MessageBox.Show(String.Format("{0} :: {1}", Name, contextItem));
            System.Windows.Forms.MessageBox.Show(String.Format("{0}", AdAddIn.Properties.Settings.Default.CdarUrl));
        }

        public override bool IsEnabled(Option<ContextItem> contextItem)
        {
            //return contextItem.Match(
            //    ci => true,
            //    () => false);
            return true;
        }
    }
}
