using AdAddIn.ADTechnology;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.PopulateDependencies
{
    class PopulateDependenciesCommand : ICommand<Option<ContextItem>, Unit>
    {
        private readonly IReadableAtom<EA.Repository> Repo;

        public PopulateDependenciesCommand(IReadableAtom<EA.Repository> repo)
        {
            Repo = repo;
        }

        public Unit Execute(Option<ContextItem> contextItem)
        {
            var elements = Repo.Val.CollectElements(e =>
            {
                return Options.Some(e);
            });

            System.Windows.Forms.MessageBox.Show(String.Format("{0}", elements.Select(e => e.ElementGUID + " " + e.Type).Join("\n")));

            GetProblemForClassifier(contextItem).Do(problem =>
            {
                var dependencies = DependencyTree.Create(Repo.Val, problem, levels: 3);
                System.Windows.Forms.MessageBox.Show(dependencies.ToString());
            });

            return Unit.Instance;
        }

        public Boolean CanExecute(Option<ContextItem> contextItem)
        {
            return GetProblemForClassifier(contextItem).IsDefined;
        }

        private Option<EA.Element> GetProblemForClassifier(Option<ContextItem> contextItem)
        {
            return from ci in contextItem
                   from e in Repo.Val.TryGetElement(ci.Guid)
                   from classifier in Repo.Val.TryGetElement(e.ClassifierID)
                   from problem in GetProblem(classifier)
                   select problem;
        }

        private Option<EA.Element> GetProblem(EA.Element classifier)
        {
            if (classifier.Is(ElementStereotypes.Problem))
                return Options.Some(classifier);
            else if (classifier.Is(ElementStereotypes.Option))
                return (from c in classifier.Connectors.Cast<EA.Connector>()
                        where c.Is(ConnectorStereotypes.HasAlternative)
                        from source in Repo.Val.TryGetElement(c.ClientID)
                        where source.Is(ElementStereotypes.Problem)
                        select source).FirstOption();
            else
                return Options.None<EA.Element>();
        }
    }
}
