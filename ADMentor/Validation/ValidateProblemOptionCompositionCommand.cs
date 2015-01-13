using ADMentor.ADTechnology;
using ADMentor.DataAccess;
using EAAddInBase;
using EAAddInBase.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace ADMentor.Validation
{
    class ValidateProblemOptionCompositionCommand : ICommand<AdEntity, Option<ValidationMessage>>
    {
        private readonly AdRepository Repo;

        public ValidateProblemOptionCompositionCommand(AdRepository repo)
        {
            Repo = repo;
        }

        public Option<ValidationMessage> Execute(AdEntity e)
        {
            return e.Match<AdEntity, Option<ValidationMessage>>()
                .Case<Problem>(p =>
                    p.GetOptions(Repo.GetElement).IsEmpty().Then(() => ValidationMessage.Warning("No associated Option")))
                .Case<OptionEntity>(o =>
                    o.GetProblems(Repo.GetElement).IsEmpty().Then(() => ValidationMessage.Warning("No associated Problem")))
                .Case<ProblemOccurrence>(po =>
                    po.GetAlternatives(Repo.GetElement).IsEmpty().Then(() => ValidationMessage.Warning("No associated Option Occurrence")))
                .Case<OptionOccurrence>(oo =>
                    oo.GetAssociatedProblemOccurrences(Repo.GetElement).IsEmpty().Then(() => ValidationMessage.Warning("No associated Problem Occurrence")))
                .GetOrThrowNotImplemented();
        }

        public bool CanExecute(AdEntity _)
        {
            return true;
        }
    }
}
