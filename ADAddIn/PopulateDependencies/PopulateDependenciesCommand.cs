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
                var dependencies = DependencyTree.Create(Repo.Val, problem, 3);
                System.Windows.Forms.MessageBox.Show(dependencies.ToString());
            });

            return Unit.Instance;
        }

        private Option<EA.Element> GetProblemForClassifier(Option<ContextItem> contextItem)
        { 
            return from ci in contextItem
                   from e in Repo.Val.TryGetElement(ci.Guid)
                   from c in Repo.Val.TryGetElement(e.ClassifierID)
                   select c;
        }

        public Boolean CanExecute(Option<ContextItem> contextItem)
        {
            return (from ci in contextItem
                    from e in Repo.Val.TryGetElement(ci.Guid)
                    select e.Is(ElementStereotypes.ProblemOccurrence) || e.Is(ElementStereotypes.Decision))
                    .GetOrElse(false);
        }
    }
}
