using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public static IEnumerable<EA.Connector> Connectors(this EA.Package p)
        {
            return p.Connectors.Cast<EA.Connector>();
        }

        public static IEnumerable<EA.Package> DescendantPackages(this EA.Package package)
        {
            return package.Packages().Aggregate(ImmutableList.Create(package), (acc, p) =>
            {
                return acc.AddRange(p.DescendantPackages());
            });
        }
    }
}
