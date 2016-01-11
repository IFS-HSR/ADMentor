using ADMentor.ADTechnology;
using EAAddInBase;
using EAAddInBase.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace ADMentor.DataAccess
{
    public abstract class SolutionEntity : AdEntity
    {
        internal SolutionEntity(EA.Element e, IEntityWrapper wrapper) : base(e, wrapper) { }
    }

    public sealed class ProblemOccurrence : SolutionEntity
    {
        internal ProblemOccurrence(EA.Element e, IEntityWrapper wrapper) : base(e, wrapper) { }

        public SolutionSpace.ProblemOccurrenceState State
        {
            get
            {
                return (from value in Get(SolutionSpace.ProblemOccurrenceStateTag)
                        join state in SolutionSpace.ProblemOccurrenceState.AllStates on value equals state.Name
                        select state)
                        .FirstOption()
                        .GetOrElse(SolutionSpace.ProblemOccurrenceState.Open);
            }
            set
            {
                Set(SolutionSpace.ProblemOccurrenceStateTag, value.Name);
                if (value == SolutionSpace.ProblemOccurrenceState.Solved && Get(Common.DecisionDate).Any(s => s.IsEmpty()))
                {
                    Set(Common.DecisionDate, DateTime.Now.ToString("d"));
                }
            }
        }

        public IEnumerable<OptionOccurrence> Alternatives(Func<int, Option<ModelEntity.Element>> getElementById)
        {
            return ElementsConnectedBy<OptionOccurrence>(ConnectorStereotypes.AddressedBy, getElementById);
        }

        public SolutionSpace.ProblemOccurrenceState DeduceState(IEnumerable<OptionOccurrence> alternatives)
        {
            var noAlternatives = alternatives.Count();
            var noCandidates = alternatives.Count(a => a.State == SolutionSpace.OptionState.Eligible);
            var noTentative = alternatives.Count(a => a.State == SolutionSpace.OptionState.Tentative);
            var noChosen = alternatives.Count(a => a.State == SolutionSpace.OptionState.Chosen);
            var noChallenged = alternatives.Count(a => a.State == SolutionSpace.OptionState.Challenged);

            if (noAlternatives == noCandidates)
                return SolutionSpace.ProblemOccurrenceState.Open;
            else if (noCandidates + noTentative + noChallenged > 0)
                return SolutionSpace.ProblemOccurrenceState.PartiallySolved;
            else if (noChosen > 0)
                return SolutionSpace.ProblemOccurrenceState.Solved;
            else
                return SolutionSpace.ProblemOccurrenceState.NotApplicable;
        }
    }

    public sealed class OptionOccurrence : SolutionEntity
    {
        internal OptionOccurrence(EA.Element e, IEntityWrapper wrapper) : base(e, wrapper) { }

        public SolutionSpace.OptionState State
        {
            get
            {
                return (from value in Get(SolutionSpace.OptionStateTag)
                        join state in SolutionSpace.OptionState.AllStates on value equals state.Name
                        select state)
                        .FirstOption()
                        .GetOrElse(SolutionSpace.OptionState.Eligible);
            }
            set
            {
                Set(SolutionSpace.OptionStateTag, value.Name);
            }
        }

        public IEnumerable<ProblemOccurrence> AssociatedProblemOccurrences(Func<int, Option<ModelEntity.Element>> getElementById)
        {
            return ElementsConnectedBy<ProblemOccurrence>(ConnectorStereotypes.AddressedBy, getElementById);
        }
    }
}
