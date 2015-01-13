using ADMentor.DataAccess;
using EAAddInBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace ADMentor.Validation
{
    public class ValidateProblemOccurrenceStateCommand : ICommand<ProblemOccurrence, Option<ValidationMessage>>
    {
        private readonly AdRepository Repo;

        public ValidateProblemOccurrenceStateCommand(AdRepository repo)
        {
            Repo = repo;
        }

        public Option<ValidationMessage> Execute(ProblemOccurrence po)
        {
            var expectedState = po.DeduceState(po.GetAlternatives(Repo.GetElement));

            return (po.State != expectedState).Then(() =>
                ValidationMessage.Error("Inconsistent State between Problem Occurrence and associated Option Occurrences"));
        }

        public bool CanExecute(ProblemOccurrence _)
        {
            return true;
        }
    }
}
