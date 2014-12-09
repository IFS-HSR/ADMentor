using AdAddIn.DataAccess;
using EAAddInFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.Validation
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
                ValidationMessage.Error("Inconsistent State"));
        }

        public bool CanExecute(ProblemOccurrence _)
        {
            return true;
        }
    }
}
