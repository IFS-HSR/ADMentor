using ADMentor.ADTechnology;
using ADMentor.DataAccess;
using EAAddInBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace ADMentor.UsabilityShortCuts
{
    /// <summary>
    /// Sets the state of the option occurrence to choosen and neglects all alternatives that are
    /// not in a final state (chosen or neglected).
    /// </summary>
    sealed class ChooseOptionOccurrenceCommand<R> : ICommand<OptionOccurrence, R>
    {
        private readonly AdRepository Repo;
        private readonly ICommand<OptionOccurrence, R> UpdateProblemOccurrenceState;

        public ChooseOptionOccurrenceCommand(AdRepository repo, ICommand<OptionOccurrence, R> updateProblemOccurrenceState)
        {
            Repo = repo;
            UpdateProblemOccurrenceState = updateProblemOccurrenceState;
        }

        public R Execute(OptionOccurrence oo)
        {
            oo.State = SolutionSpace.OptionState.Chosen;
            Repo.PropagateChanges(oo);

            (from po in oo.AssociatedProblemOccurrences(Repo.GetElement)
             from otherOo in po.Alternatives(Repo.GetElement)
             where !oo.Equals(otherOo)
             select otherOo).ForEach(otherOo =>
             {
                 if (otherOo.State != SolutionSpace.OptionState.Chosen && otherOo.State != SolutionSpace.OptionState.Neglected)
                 {
                     otherOo.State = SolutionSpace.OptionState.Neglected;
                     Repo.PropagateChanges(otherOo);
                 }
             });

            return UpdateProblemOccurrenceState.Execute(oo);
        }

        public bool CanExecute(OptionOccurrence oo)
        {
            return UpdateProblemOccurrenceState.CanExecute(oo);
        }
    }
}
