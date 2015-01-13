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

    public class Problem : ProblemSpaceEntity
    {
        internal Problem(EA.Element e, IEntityWrapper wrapper) : base(e, wrapper) { }

        public IEnumerable<OptionEntity> GetOptions(Func<int, Option<ModelEntity.Element>> getElementById)
        {
            return from connector in Connectors
                   where connector.Is(ConnectorStereotypes.AddressedBy)
                   from source in connector.OppositeEnd(this, getElementById)
                   from option in source.TryCast<OptionEntity>()
                   select option;
        }
    }

    public class OptionEntity : ProblemSpaceEntity
    {
        internal OptionEntity(EA.Element e, IEntityWrapper wrapper) : base(e, wrapper) { }

        public IEnumerable<Problem> GetProblems(Func<int, Option<ModelEntity.Element>> getElementById)
        {
            return from connector in Connectors
                   where connector.Is(ConnectorStereotypes.AddressedBy)
                   from source in connector.OppositeEnd(this, getElementById)
                   from problem in source.TryCast<Problem>()
                   select problem;
        }
    }
}
