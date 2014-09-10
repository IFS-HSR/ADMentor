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
    public class OptionOccurrence : AdEntity
    {
        internal OptionOccurrence(EA.Element e, IEntityWrapper wrapper) : base(e, wrapper) { }

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
                EaObject.Set(Solution.OptionStateTag, value.Name);
            }
        }

        public IEnumerable<ProblemOccurrence> GetAssociatedProblemOccurrences(Func<int, Option<ModelEntity.Element>> getElementById)
        {
            return from connector in EaObject.Connectors()
                   where connector.Is(ConnectorStereotypes.HasAlternative)
                   from source in getElementById(connector.ClientID)
                   from problemOcc in Wrapper.Wrap(source.EaObject).Match<ProblemOccurrence>()
                   select problemOcc;
        }
    }
}
