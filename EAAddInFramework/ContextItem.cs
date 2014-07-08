using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInFramework
{
    public class ContextItem
    {
        public ContextItem(EA.ObjectType type, string guid)
        {
            Type = type;
            Guid = guid;
        }

        public EA.ObjectType Type { get; private set; }

        public String Guid { get; private set; }

        public override string ToString()
        {
            return String.Format("ContextItem({0}, {1})", Type, Guid);
        }
    }
}
