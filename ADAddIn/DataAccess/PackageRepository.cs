using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.DataAccess
{
    public class PackageRepository
    {
        private readonly IReadableAtom<EA.Repository> EA;

        public PackageRepository(IReadableAtom<EA.Repository> ea)
        {
            EA = ea;
        }

        public Option<EA.Package> GetPackage(int id)
        {
            return EA.Val.GetPackageByID(id).AsOption();
        }

        public Option<EA.Package> GetPackage(string guid)
        {
            return EA.Val.GetPackageByGuid(guid).AsOption();
        }

        public Option<EA.Package> Find(Func<EA.Package, bool> pred)
        {
            return AggregatePackages(Options.None<EA.Package>(), (acc, package) =>
            {
                if (pred(package))
                    return Options.Some(package);
                else
                    return acc;
            });
        }

        public void Traverse(EA.Package start, Action<EA.Package> act)
        {
            AggregatePackages(start, Unit.Instance, (acc, package) =>
            {
                act(package);
                return acc;
            });
        }

        public T AggregatePackages<T>(T init, Func<T, EA.Package, T> fn)
        {
            return EA.Val.Models.Cast<EA.Package>().Aggregate(init, (acc, package) =>
            {
                return AggregatePackages(package, acc, fn);
            });
        }

        public T AggregatePackages<T>(EA.Package package, T init, Func<T, EA.Package, T> fn)
        {
            var res = fn(init, package);
            return package.Packages.Cast<EA.Package>().Aggregate(res, (acc, child) =>
            {
                return AggregatePackages(child, acc, fn);
            });
        }

        internal EA.Package Create(EA.Package parentPackage, string name)
        {
            var pkg = parentPackage.Packages.AddNew(name, "") as EA.Package;
            pkg.Update();
            parentPackage.Packages.Refresh();
            return pkg;
        }
    }
}
