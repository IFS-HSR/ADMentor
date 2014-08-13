using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace EAAddInFramework.MDGBuilder
{
    public static class LookupHelpers
    {
        public static Option<ConnectorStereotype> GetStereotype(this EA.Connector c)
        {
            return ConnectorStereotype.Instances.FirstOption(stype => c.Stereotype == stype.Name);
        }

        public static Option<ElementStereotype> GetStereotype(this EA.Element c)
        {
            return ElementStereotype.Instances.FirstOption(stype => c.Stereotype == stype.Name);
        }
    }
}
