using ADMentor.ADTechnology;
using ADMentor.DataAccess;
using EAAddInBase;
using EAAddInBase.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace ADMentor.Navigation
{
    /// <summary>
    /// Navigate to the element that has been used to instantiate the selected element.
    /// This command is only executable if an element is selected and the element is an
    /// invocation/instance of another element.
    /// </summary>
    sealed class GoToClassifierCommand : ICommand<ModelEntity.Element, Unit>
    {
        private readonly AdRepository Repo;

        public GoToClassifierCommand(AdRepository repo)
        {
            Repo = repo;
        }

        public Unit Execute(ModelEntity.Element contextItem)
        {
            contextItem.GetClassifier(Repo.GetElement).Do(c =>
                Repo.ShowInProjectView(c));

            return Unit.Instance;
        }

        public bool CanExecute(ModelEntity.Element contextItem)
        {
            return contextItem.GetClassifier(Repo.GetElement).IsDefined;
        }
    }
}
