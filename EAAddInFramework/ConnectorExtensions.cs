using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInFramework
{
    public static class ConnectorExtensions
    {
        public static bool Is(this EA.Connector c, ConnectorStereotype stereotype)
        {
            return c.Stereotype == stereotype.Name && c.Type == stereotype.Type.Name;
        }

        public static IEnumerable<EA.TaggedValue> TaggedValues(this EA.Connector c)
        {
            return c.TaggedValues.Cast<EA.TaggedValue>();
        }
    }
}
