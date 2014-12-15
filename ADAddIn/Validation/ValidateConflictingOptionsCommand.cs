using AdAddIn.ADTechnology;
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
    public class ValidateConflictingOptionsCommand : ICommand<ProblemOccurrence, Option<ValidationMessage>>
    {
        private readonly AdRepository Repo;

        public ValidateConflictingOptionsCommand(AdRepository repo)
        {
            Repo = repo;
        }

        public Option<ValidationMessage> Execute(ProblemOccurrence po)
        {
            var chosenOos = po.GetAlternatives(Repo.GetElement)
                .Where(oo => oo.State == SolutionSpace.OptionState.Chosen)
                .Run();

            return (from oo in chosenOos
                    from c in oo.Connectors
                    where c.Is(ConnectorStereotypes.ConflictsWith)
                    from otherEnd in c.OppositeEnd(oo, Repo.GetElement)
                    where chosenOos.Contains(otherEnd)
                    select otherEnd)
                    .Any()
                    .Then(() => ValidationMessage.Error("Conflicting options chosen"));
        }

        public bool CanExecute(ProblemOccurrence _)
        {
            return true;
        }
    }
}
