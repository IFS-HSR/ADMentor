using AdAddIn.ADTechnology;
using AdAddIn.ExportProblemSpace;
using EAAddInFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.CopyMetadata
{
    public class ProblemOccurrence : ModelEntity.Element
    {
        private ProblemOccurrence(EA.Element e) : base(e) { }

        public static Option<ProblemOccurrence> Wrap(EA.Element element)
        {
            return from e in element.AsOption()
                   where e.Is(Solution.ProblemOccurrence)
                   select new ProblemOccurrence(e);
        }

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
                Val.Set(Solution.ProblemOccurrenceStateTag, value.Name);
            }
        }

        public IEnumerable<OptionOccurrence> GetAlternatives(Func<int, Option<EA.Element>> getElementById)
        {
            return from connector in Val.Connectors()
                   where connector.Is(ConnectorStereotypes.HasAlternative)
                   from target in getElementById(connector.SupplierID)
                   from optionOcc in OptionOccurrence.Wrap(target)
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
