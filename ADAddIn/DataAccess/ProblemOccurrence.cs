using AdAddIn.ADTechnology;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.DataAccess
{
    public class ProblemOccurrence : AdEntity
    {
        internal ProblemOccurrence(EA.Element e, IEntityWrapper wrapper) : base(e, wrapper) { }

        public Solution.ProblemOccurrenceState State
        {
            get
            {
                return (from value in Get(Solution.ProblemOccurrenceStateTag)
                        join state in Solution.ProblemOccurrenceState.AllStates on value equals state.Name
                        select state)
                        .FirstOption()
                        .GetOrElse(Solution.ProblemOccurrenceState.Open);
            }
            set
            {
                EaObject.Set(Solution.ProblemOccurrenceStateTag, value.Name);
            }
        }

        public IEnumerable<OptionOccurrence> GetAlternatives(Func<int, Option<ModelEntity.Element>> getElementById)
        {
            return from connector in EaObject.Connectors()
                   where connector.Is(ConnectorStereotypes.HasAlternative)
                   from target in getElementById(connector.SupplierID)
                   from optionOcc in Wrapper.Wrap(target.EaObject).Match<OptionOccurrence>()
                   select optionOcc;
        }

        public Solution.ProblemOccurrenceState DeduceState(IEnumerable<OptionOccurrence> alternatives)
        {
            var noAlternatives = alternatives.Count();
            var noCandidates = alternatives.Count(a => a.State == Solution.OptionState.Candidate);
            var noChosen = alternatives.Count(a => a.State == Solution.OptionState.Chosen);
            var noNeglected = alternatives.Count(a => a.State == Solution.OptionState.Neglected);

            if (noAlternatives == noCandidates)
                return Solution.ProblemOccurrenceState.Open;
            else if (noCandidates > 0 && noCandidates < noAlternatives)
                return Solution.ProblemOccurrenceState.PartiallySolved;
            else if (noChosen > 0 && noCandidates == 0)
                return Solution.ProblemOccurrenceState.Solved;
            else
                return Solution.ProblemOccurrenceState.NotApplicable;
        }
    }
}
