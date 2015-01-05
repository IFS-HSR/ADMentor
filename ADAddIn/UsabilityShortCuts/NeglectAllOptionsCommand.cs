using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using EAAddInFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.UsabilityShortCuts
{
    public class NeglectAllOptionsCommand : ICommand<ProblemOccurrence, Unit>
    {
        private readonly AdRepository Repo;

        public NeglectAllOptionsCommand(AdRepository repo)
        {
            Repo = repo;
        }

        public Unit Execute(ProblemOccurrence po)
        {
            var alternatives = po.GetAlternatives(Repo.GetElement).Select(oo =>
            {
                oo.State = SolutionSpace.OptionState.Neglected;
                Repo.PropagateChanges(oo);
                return oo;
            });

            po.State = po.DeduceState(alternatives);
            Repo.PropagateChanges(po);

            return Unit.Instance;
        }

        public bool CanExecute(ProblemOccurrence _)
        {
            return true;
        }
    }
}
