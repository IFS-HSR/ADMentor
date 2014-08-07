using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace EAAddInFramework
{
    using CustomDetailViewCommand = ICommand<Func<EA.Element>, EntityModified>;

    public class EntityModified
    {
        public static readonly EntityModified Modified = new EntityModified(true);
        public static readonly EntityModified NotModified = new EntityModified(false);

        private EntityModified(bool val) { Val = val; }

        public bool Val { get; private set; }
    }

    class CustomDetailViewHandler
    {
        private IReadableAtom<EA.Repository> repository;
        private IList<CustomDetailViewCommand> customDetailViewCommands = new List<CustomDetailViewCommand>();

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public CustomDetailViewHandler(IReadableAtom<EA.Repository> repository)
        {
            this.repository = repository;
        }

        public void Register(CustomDetailViewCommand command)
        {
            customDetailViewCommands.Add(command);
        }

        public EntityModified CallElementDetailViews(Func<EA.Element> getElement)
        {
            return customDetailViewCommands.Aggregate(EntityModified.NotModified,
                (modified, cmd) =>
                {
                    if (cmd.CanExecute(getElement))
                    {
                        var res = cmd.Execute(getElement);
                        return modified == EntityModified.Modified ? modified : res;
                    }
                    else
                    {
                        return modified;
                    }
                });
        }
    }
}
