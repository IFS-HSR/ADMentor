using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdAddIn.DataAccess
{
    public abstract class ProblemSpaceEntity : AdEntity
    {
        internal ProblemSpaceEntity(EA.Element e, IEntityWrapper wrapper) : base(e, wrapper) { }
    }

    public class Problem : ProblemSpaceEntity
    {
        internal Problem(EA.Element e, IEntityWrapper wrapper) : base(e, wrapper) { }
    }

    public class OptionEntity : ProblemSpaceEntity
    {
        internal OptionEntity(EA.Element e, IEntityWrapper wrapper) : base(e, wrapper) { }
    }
}
