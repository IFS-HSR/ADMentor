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
    public class OptionOccurrence : ModelEntity.Element
    {
        private OptionOccurrence(EA.Element e) : base(e) { }

        public static Option<OptionOccurrence> Wrap(EA.Element element)
        {
            return from e in element.AsOption()
                   where e.Is(Solution.OptionOccurrence)
                   select new OptionOccurrence(e);
        }

        public Solution.OptionState State
        {
            get
            {
                return (from value in Get(Solution.OptionStateTag)
                        join state in Solution.OptionState.AllStates on value equals state.Name
                        select state)
                        .FirstOption()
                        .GetOrElse(Solution.OptionState.Candidate);
            }
            set
            {
                Val.Set(Solution.OptionStateTag, value.Name);
            }
        }

        public IEnumerable<ProblemOccurrence> GetAssociatedProblemOccurrences(Func<int, Option<EA.Element>> getElementById)
        {
            return from connector in Val.Connectors()
                   where connector.Is(ConnectorStereotypes.HasAlternative)
                   from source in getElementById(connector.ClientID)
                   from problemOcc in ProblemOccurrence.Wrap(source)
                   select problemOcc;
        }
    }
}
