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
    sealed class ValidateProblemOptionCompositionCommand : ICommand<AdEntity, Option<ValidationMessage>>
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
                    p.Options(Repo.GetElement).IsEmpty().Then(() => ValidationMessage.Warning("No associated Option")))
                .Case<OptionEntity>(o =>
                    o.Problems(Repo.GetElement).IsEmpty().Then(() => ValidationMessage.Warning("No associated Problem")))
                .Case<ProblemOccurrence>(po =>
                    po.Alternatives(Repo.GetElement).IsEmpty().Then(() => ValidationMessage.Warning("No associated Option Occurrence")))
                .Case<OptionOccurrence>(oo =>
                    oo.AssociatedProblemOccurrences(Repo.GetElement).IsEmpty().Then(() => ValidationMessage.Warning("No associated Problem Occurrence")))
                .GetOrThrowNotImplemented();
        }

        public bool CanExecute(AdEntity _)
        {
            return true;
        }
    }
}
