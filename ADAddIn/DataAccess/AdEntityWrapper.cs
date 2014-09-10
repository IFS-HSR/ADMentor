using EAAddInFramework.DataAccess;
using EAAddInFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdAddIn.ADTechnology;

namespace AdAddIn.DataAccess
{
    public class AdEntityWrapper : EntityWrapper
    {
        public override ModelEntity.Element Wrap(EA.Element e)
        {
            if (e.Is(Solution.OptionOccurrence))
                return new OptionOccurrence(e, this);
            else if (e.Is(Solution.ProblemOccurrence))
                return new ProblemOccurrence(e, this);
            else if (e.Is(ProblemSpace.Problem))
                return new Problem(e, this);
            else if (e.Is(ProblemSpace.Option))
                return new OptionEntity(e, this);
            else
                return base.Wrap(e);
        }
    }
}
