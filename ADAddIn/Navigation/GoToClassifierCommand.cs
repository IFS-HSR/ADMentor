using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
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
    /// Navigate to the element that has been used to instantiate the selected element.
    /// This command is only executable if an element is selected and the element is an
    /// invocation/instance of another element.
    /// </summary>
    public class GoToClassifierCommand : ICommand<Option<ContextItem>, Unit>
    {
        private readonly ElementRepository Repo;

        public GoToClassifierCommand(ElementRepository repo)
        {
            Repo = repo;
        }

        public Unit Execute(Option<ContextItem> contextItem)
        {
            GetClassifier(contextItem).Do(c =>
            {
                Repo.EA.Val.ShowInProjectView(c);
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
                   from element in Repo.GetElement(ci.Guid)
                   from classifier in Repo.GetClassifier(element)
                   select classifier;
        }
    }
}
