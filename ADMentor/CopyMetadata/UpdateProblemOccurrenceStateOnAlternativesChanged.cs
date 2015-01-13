using ADMentor.ADTechnology;
using ADMentor.DataAccess;
using ADMentor.PopulateDependencies;
using EAAddInBase;
using EAAddInBase.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace ADMentor.CopyMetadata
{
    public class UpdateProblemOccurrenceStateOnAlternativesChanged : ICommand<OptionOccurrence, Unit>
    {
        private readonly ModelEntityRepository Repo;

        public UpdateProblemOccurrenceStateOnAlternativesChanged(ModelEntityRepository repo)
        {
            Repo = repo;
        }

        public Unit Execute(OptionOccurrence optionOcc)
        {
            var problemOccs = from problemOcc in optionOcc.GetAssociatedProblemOccurrences(Repo.GetElement)
                              select problemOcc;

            problemOccs.ForEach(problemOcc =>
            {
                var alternatives = problemOcc.GetAlternatives(Repo.GetElement);
                problemOcc.State = problemOcc.DeduceState(alternatives);
                Repo.PropagateChanges(problemOcc);
            });

            return Unit.Instance;
        }

        public bool CanExecute(OptionOccurrence _)
        {
            return true;
        }
    }
}
