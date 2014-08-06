using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInFramework.DataAccess
{
    public static class StereotypeHelpers
    {
        public static EA.Element Create(this ElementStereotype stereotype, EA.Package package, String name)
        {
            var e = package.Elements.AddNew(name, stereotype.Type.Name) as EA.Element;

            e.Stereotype = stereotype.Name;
            e.Update();
            package.Elements.Refresh();

            return e;
        }

        public static EA.Connector Create(this ConnectorStereotype stereotype, EA.Element source, EA.Element target, String name = "")
        {
            var c = source.Connectors.AddNew(name, stereotype.Type.Name) as EA.Connector;

            c.Stereotype = stereotype.Name;
            c.SupplierID = target.ElementID;
            c.Update();
            source.Connectors.Refresh();
            target.Connectors.Refresh();

            return c;
        }
    }
}
