using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using EAAddInFramework;
using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.Validation
{
    class ValidateProblemOptionComposition : ICommand<AdEntity, Option<ValidationMessage>>
    {
        private readonly AdRepository Repo;

        public ValidateProblemOptionComposition(AdRepository repo)
        {
            Repo = repo;
        }

        public Option<ValidationMessage> Execute(AdEntity e)
        {
            return e.Match<Problem>().SelectMany(ValidateProblem)
                .OrElse(() => e.Match<OptionEntity>().SelectMany(ValidateOption))
                .OrElse(() => e.Match<ProblemOccurrence>().SelectMany(ValidateProblemOcc))
                .OrElse(() => e.Match<OptionOccurrence>().SelectMany(ValidateOptionOcc));
        }

        public Option<ValidationMessage> ValidateProblem(Problem p)
        {
            return p.GetOptions(Repo.GetElement).IsEmpty().Then(() => ValidationMessage.Warning("No associated Option"));
        }

        public Option<ValidationMessage> ValidateOption(OptionEntity o)
        {
            return o.GetProblems(Repo.GetElement).IsEmpty().Then(() => ValidationMessage.Warning("No associated Problem"));
        }

        public Option<ValidationMessage> ValidateProblemOcc(ProblemOccurrence p)
        {
            return p.GetAlternatives(Repo.GetElement).IsEmpty().Then(() => ValidationMessage.Warning("No associated Option Occurrence"));
        }

        public Option<ValidationMessage> ValidateOptionOcc(OptionOccurrence o)
        {
            return o.GetAssociatedProblemOccurrences(Repo.GetElement).IsEmpty().Then(() => ValidationMessage.Warning("No associated Problem Occurrence"));
        }

        public bool CanExecute(AdEntity _)
        {
            return true;
        }
    }
}
