using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInBase.DataAccess
{
    public class DiagramObject
    {
        public DiagramObject(EA.DiagramObject eaObject)
        {
            EaObject = eaObject;
        }

        public EA.DiagramObject EaObject { get; private set; }
    }
}
