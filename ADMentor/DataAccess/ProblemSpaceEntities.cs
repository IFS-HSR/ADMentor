using ADMentor.ADTechnology;
using EAAddInBase.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace ADMentor.DataAccess
{
    public abstract class ProblemSpaceEntity : AdEntity
    {
        internal ProblemSpaceEntity(EA.Element e, IEntityWrapper wrapper) : base(e, wrapper) { }
    }

    public sealed class Problem : ProblemSpaceEntity
    {
        internal Problem(EA.Element e, IEntityWrapper wrapper) : base(e, wrapper) { }

        public IEnumerable<OptionEntity> Options(Func<int, Option<ModelEntity.Element>> getElementById)
        {
            return ElementsConnectedBy<OptionEntity>(ConnectorStereotypes.AddressedBy, getElementById);
        }
    }

    public sealed class OptionEntity : ProblemSpaceEntity
    {
        internal OptionEntity(EA.Element e, IEntityWrapper wrapper) : base(e, wrapper) { }

        public IEnumerable<Problem> Problems(Func<int, Option<ModelEntity.Element>> getElementById)
        {
            return ElementsConnectedBy<Problem>(ConnectorStereotypes.AddressedBy, getElementById);
        }
    }
}
