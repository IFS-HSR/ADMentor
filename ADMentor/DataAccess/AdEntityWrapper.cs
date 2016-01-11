using EAAddInBase.DataAccess;
using EAAddInBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADMentor.ADTechnology;
using EAAddInBase.MDGBuilder;

namespace ADMentor.DataAccess
{
    public class AdEntityWrapper : EntityWrapper
    {
        public override ModelEntity.Element Wrap(EA.Element e)
        {
            if (StereotypeIs(e, SolutionSpace.OptionOccurrence))
                return new OptionOccurrence(e, this);
            else if (StereotypeIs(e, SolutionSpace.ProblemOccurrence))
                return new ProblemOccurrence(e, this);
            else if (StereotypeIs(e, ProblemSpace.Problem))
                return new Problem(e, this);
            else if (StereotypeIs(e, ProblemSpace.Option))
                return new OptionEntity(e, this);
            else
                return base.Wrap(e);
        }

        private bool StereotypeIs(EA.Element e, ElementStereotype stype)
        {
            return e.Type.Equals(stype.Type.Name) && e.Stereotype.Equals(stype.Name);
        }
    }
}
