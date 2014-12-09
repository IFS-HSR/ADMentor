using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
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
    public class GoToClassifierCommand : ICommand<Option<ModelEntity>, Unit>
    {
        private readonly IReadableAtom<EA.Repository> EARepo;

        public GoToClassifierCommand(IReadableAtom<EA.Repository> eaRepo)
        {
            EARepo = eaRepo;
        }

        public Unit Execute(Option<ModelEntity> contextItem)
        {
            GetClassifier(contextItem).Do(c =>
            {
                EARepo.Val.ShowInProjectView(c);
            });

            return Unit.Instance;
        }

        public bool CanExecute(Option<ModelEntity> contextItem)
        {
            return GetClassifier(contextItem).IsDefined;
        }

        private Option<EA.Element> GetClassifier(Option<ModelEntity> contextItem)
        {
            return from ci in contextItem
                   from element in ci.TryCast<ModelEntity.Element>()
                   from classifier in Options.Try(() => EARepo.Val.GetElementByID(element.EaObject.ClassifierID))
                   select classifier;
        }
    }
}
