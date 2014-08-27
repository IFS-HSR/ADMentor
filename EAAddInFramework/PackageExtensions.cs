using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInFramework
{
    public static class PackageExtensions
    {
        public static IEnumerable<EA.Package> Packages(this EA.Package p)
        {
            return p.Packages.Cast<EA.Package>();
        }

        public static IEnumerable<EA.Diagram> Diagrams(this EA.Package p)
        {
            return p.Diagrams.Cast<EA.Diagram>();
        }

        public static IEnumerable<EA.Element> Elements(this EA.Package p)
        {
            return p.Elements.Cast<EA.Element>();
        }
    }
}
