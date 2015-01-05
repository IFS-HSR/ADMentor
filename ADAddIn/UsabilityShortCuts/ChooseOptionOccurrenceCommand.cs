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
    public class ChooseOptionOccurrenceCommand : ICommand<OptionOccurrence, Unit>
    {
        private readonly AdRepository Repo;
        private readonly ICommand<OptionOccurrence, Unit> UpdateProblemOccurrenceState;

        public ChooseOptionOccurrenceCommand(AdRepository repo, ICommand<OptionOccurrence, Unit> updateProblemOccurrenceState)
        {
            Repo = repo;
            UpdateProblemOccurrenceState = updateProblemOccurrenceState;
        }

        public Unit Execute(OptionOccurrence oo)
        {
            oo.State = SolutionSpace.OptionState.Chosen;
            Repo.PropagateChanges(oo);

            (from po in oo.GetAssociatedProblemOccurrences(Repo.GetElement)
             from otherOo in po.GetAlternatives(Repo.GetElement)
             where !oo.Equals(otherOo)
             select otherOo).ForEach(otherOo =>
             {
                 if (otherOo.State != SolutionSpace.OptionState.Chosen && otherOo.State != SolutionSpace.OptionState.Neglected)
                 {
                     otherOo.State = SolutionSpace.OptionState.Neglected;
                     Repo.PropagateChanges(otherOo);
                 }
             });

            UpdateProblemOccurrenceState.Execute(oo);

            return Unit.Instance;
        }

        public bool CanExecute(OptionOccurrence _)
        {
            return true;
        }
    }
}
