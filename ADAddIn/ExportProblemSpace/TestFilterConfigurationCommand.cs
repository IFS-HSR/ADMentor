using EAAddInFramework;
using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.ExportProblemSpace
{
    class TestFilterConfigurationCommand : ICommand<ModelEntity.Package, Unit>
    {
        public Unit Execute(ModelEntity.Package p)
        {
            var form = new ElementFilterConfiguration();

            form.Display();

            return Unit.Instance;
        }

        public bool CanExecute(ModelEntity.Package p)
        {
            return true;
        }

        public ICommand<Option<ModelEntity>, object> AsMenuCommand()
        {
            return this.Adapt((Option<ModelEntity> contextItem) =>
            {
                return from ci in contextItem
                       from package in ci.Match<ModelEntity.Package>()
                       select package;
            });
        }
    }
}
