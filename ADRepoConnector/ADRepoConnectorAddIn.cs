using AdAddIn.DataAccess;
using AdAddIn.ADTechnology;
using EAAddInFramework;
using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using EAAddInFramework.DataAccess;

namespace ADRepoConnector
{
    public class ADRepoConnectorAddIn : EAAddIn
    {
        public override string AddInName
        {
            get { return "AD Repo Connector"; }
        }

        public override Tuple<Option<IEntityWrapper>, IEnumerable<ValidationRule>> Bootstrap(IReadableAtom<EA.Repository> repository)
        {
            var entityWrapper = new AdEntityWrapper();
            var entityRepository = new AdRepository(repository, entityWrapper);

            var exportCmd = new ExportToADRepoCommand(entityRepository);

            Register(new Menu("AD Repo Connector",
                new MenuItem("Export to AD Repo", exportCmd.ToMenuHandler())));

            return Tuple.Create(
                Options.Some(entityWrapper as IEntityWrapper),
                Enumerable.Empty<ValidationRule>());
        }
    }
}
