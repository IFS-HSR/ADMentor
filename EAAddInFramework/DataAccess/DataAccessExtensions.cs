using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInFramework.DataAccess
{
    public static class DataAccessExtensions
    {
        public static IEnumerable<T> CollectElements<T>(this EA.Repository repo, Func<EA.Element, IEnumerable<T>> fn)
        {
            return repo.CollectPackages(
                p => p.Elements.Cast<EA.Element>().SelectMany(fn));
        }

        public static IEnumerable<T> CollectConnectors<T>(this EA.Repository repo, Func<EA.Connector, IEnumerable<T>> fn)
        {
            return repo.CollectPackages(
                p => p.Connectors.Cast<EA.Connector>().SelectMany(fn));
        }

        public static IEnumerable<T> CollectPackages<T>(this EA.Repository repo, Func<EA.Package, IEnumerable<T>> fn)
        {
            return from model in repo.Models.Cast<EA.Package>()
                   from t in model.CollectPackages(fn)
                   select t;
        }

        public static IEnumerable<T> CollectPackages<T>(this EA.Package package, Func<EA.Package, IEnumerable<T>> fn)
        {
            return fn(package)
                .Concat(from p in package.Packages.Cast<EA.Package>()
                        from t in p.CollectPackages(fn)
                        select t);
        }
    }
}
