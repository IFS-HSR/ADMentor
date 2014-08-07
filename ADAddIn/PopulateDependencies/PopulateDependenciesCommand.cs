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
                   from element in Repo.Val.TryGetElement(ci.Guid)
                   from classifier in Repo.Val.TryGetElement(element.ClassifierID)
                   let dependencyTree = DependencyTree.Create(Repo.Val, classifier, DependencyTree.TraverseFromAlternativeToProblem, 1)
                   from problem in dependencyTree.FirstOption(e => e.Is(ElementStereotypes.Problem))
                   select problem;
        }
    }
}
