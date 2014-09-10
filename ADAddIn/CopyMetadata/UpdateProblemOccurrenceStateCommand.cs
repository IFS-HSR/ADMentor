using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using AdAddIn.PopulateDependencies;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
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
        private readonly ModelEntityRepository Repo;

        public UpdateProblemOccurrenceStateCommand(ModelEntityRepository repo)
        {
            Repo = repo;
        }

        public Unit Execute(Func<EA.Element> getElement)
        {
            var problemOccs = from optionOcc in Repo.Wrapper.Wrap(getElement()).Match<OptionOccurrence>()
                              from problemOcc in optionOcc.GetAssociatedProblemOccurrences(Repo.GetElement)
                              select problemOcc;

            problemOccs.ForEach(problemOcc =>
            {
                var alternatives = problemOcc.GetAlternatives(Repo.GetElement);
                problemOcc.State = problemOcc.DeduceState(alternatives);
                Repo.PropagateChanges(problemOcc);
            });

            return Unit.Instance;
        }

        public bool CanExecute(Func<EA.Element> _)
        {
            return true;
        }
    }
}
