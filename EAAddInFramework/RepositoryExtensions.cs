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
    }
}
