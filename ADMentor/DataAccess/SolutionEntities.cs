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

    public class ProblemOccurrence : SolutionEntity
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

        public IEnumerable<OptionOccurrence> GetAlternatives(Func<int, Option<ModelEntity.Element>> getElementById)
        {
            return from connector in Connectors
                   where connector.Is(ConnectorStereotypes.AddressedBy)
                   from target in connector.Target(getElementById)
                   from optionOcc in target.TryCast<OptionOccurrence>()
                   select optionOcc;
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

    public class OptionOccurrence : SolutionEntity
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

        public IEnumerable<ProblemOccurrence> GetAssociatedProblemOccurrences(Func<int, Option<ModelEntity.Element>> getElementById)
        {
            return from connector in Connectors
                   where connector.Is(ConnectorStereotypes.AddressedBy)
                   from source in connector.OppositeEnd(this, getElementById)
                   from problemOcc in source.TryCast<ProblemOccurrence>()
                   select problemOcc;
        }
    }
}
