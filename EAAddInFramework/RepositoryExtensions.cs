using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace EAAddInFramework
{
    public static class RepositoryExtensions
    {
        public static Option<EA.Element> TryGetElement(this EA.Repository repo, String guid)
        {
            return Options.Try(() =>
            {
                return repo.GetElementByGuid(guid);
            });
        }

        public static Option<EA.Element> TryGetElement(this EA.Repository repo, int id)
        {
            return Options.Try(() =>
            {
                return repo.GetElementByID(id);
            });
        }

        public static IEnumerable<EA.Package> AllPackages(this EA.Repository repo)
        {
            return from model in repo.Models.Cast<EA.Package>()
                   from pkg in new[] { model }.Concat(AllPackages(model))
                   select pkg;
        }

        private static IEnumerable<EA.Package> AllPackages(EA.Package model)
        {
            return from pkg in model.Packages.Cast<EA.Package>()
                   from all in new[] { pkg }.Concat(AllPackages(pkg))
                   select all;
        }
    }
}
