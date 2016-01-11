using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInBase.DataAccess
{
    public interface IEntityWrapper
    {
        ModelEntity.Package Wrap(EA.Package p);
        ModelEntity.Element Wrap(EA.Element e);
        ModelEntity.Connector Wrap(EA.Connector c);
        ModelEntity.Diagram Wrap(EA.Diagram d);
        DiagramObject Wrap(EA.DiagramObject obj);
    }

    public class EntityWrapper : IEntityWrapper{
        public virtual ModelEntity.Package Wrap(EA.Package p)
        {
            return new ModelEntity.Package(p, this);
        }

        public virtual ModelEntity.Element Wrap(EA.Element e)
        {
            return new ModelEntity.Element(e, this);
        }

        public virtual ModelEntity.Connector Wrap(EA.Connector c)
        {
            return new ModelEntity.Connector(c, this);
        }

        public virtual ModelEntity.Diagram Wrap(EA.Diagram d)
        {
            return new ModelEntity.Diagram(d, this);
        }

        public virtual DiagramObject Wrap(EA.DiagramObject obj)
        {
            return new DiagramObject(obj);
        }
    }
}
