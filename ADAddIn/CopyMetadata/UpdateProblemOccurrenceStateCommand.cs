using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using AdAddIn.PopulateDependencies;
using EAAddInFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.CopyMetadata
{
    public class UpdateProblemOccurrenceStateCommand : ICommand<Func<EA.Element>, Unit>
    {
        private readonly ElementRepository ElementRepo;
        private readonly DiagramRepository DiagramRepo;

        public UpdateProblemOccurrenceStateCommand(ElementRepository elementRepo, DiagramRepository diagramRepo)
        {
            ElementRepo = elementRepo;
            DiagramRepo = diagramRepo;
        }

        public Unit Execute(Func<EA.Element> getElement)
        {
            var problemOccs = from optionOcc in OptionOccurrence.Wrap(getElement())
                              from problemOcc in optionOcc.GetAssociatedProblemOccurrences(ElementRepo.GetElement)
                              select problemOcc;

            problemOccs.ForEach(problemOcc =>
            {
                var alternatives = problemOcc.GetAlternatives(ElementRepo.GetElement);
                problemOcc.State = problemOcc.DeduceState(alternatives);
                ElementRepo.ElementChanged(problemOcc.Val);
            });

            return Unit.Instance;
        }

        public bool CanExecute(Func<EA.Element> _)
        {
            return true;
        }
    }
}
