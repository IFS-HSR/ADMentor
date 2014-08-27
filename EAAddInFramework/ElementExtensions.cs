using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInFramework
{
    public static class ElementExtensions
    {
        public static bool Is(this EA.Element e, ElementStereotype stereotype)
        {
            return e.Stereotype == stereotype.Name && e.Type == stereotype.Type.Name;
        }

        public static bool IsInstance(this EA.Element e)
        {
            return e.ClassifierID != 0;
        }

        public static bool IsNew(this EA.Element e)
        {
            return DateTime.Now - e.Created < TimeSpan.FromSeconds(1);
        }

        public static EA.Package FindPackage(this EA.Element element, EA.Repository repo)
        {
            return repo.AllPackages().First(
                pkg => pkg.Elements.Cast<EA.Element>().Any(
                    e => e.ElementID == element.ElementID));
        }

        public static IEnumerable<EA.Connector> Connectors(this EA.Element e)
        {
            return e.Connectors.Cast<EA.Connector>();
        }
    }
}
