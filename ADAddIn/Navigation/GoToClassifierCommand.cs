using AdAddIn.ADTechnology;
using EAAddInFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.Navigation
{
    /// <summary>
    /// Navigate to the element that has been used to instantiate the selected element
    /// if possible.
    /// </summary>
    public class GoToClassifierCommand : ICommand<Option<ContextItem>, Unit>
    {
        private readonly IReadableAtom<EA.Repository> Repo;

        public GoToClassifierCommand(IReadableAtom<EA.Repository> repo)
        {
            Repo = repo;
        }

        public Unit Execute(Option<ContextItem> contextItem)
        {
            GetClassifier(contextItem).Do(c =>
            {
                Repo.Val.ShowInProjectView(c);
            });

            return Unit.Instance;
        }

        public bool CanExecute(Option<ContextItem> contextItem)
        {
            return GetClassifier(contextItem).IsDefined;
        }

        private Option<EA.Element> GetClassifier(Option<ContextItem> contextItem)
        {
            return from ci in contextItem
                   from element in Repo.Val.TryGetElement(ci.Guid)
                   from classifier in Repo.Val.TryGetElement(element.ClassifierID)
                   select classifier;
        }
    }
}
