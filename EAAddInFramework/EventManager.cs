using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInFramework
{
    public class EventManager<T, R>
    {
        private readonly R InitVal;

        private readonly Func<R, R, R> Accumulate;

        private readonly IList<ICommand<T, R>> Handlers;

        internal EventManager(R initVal, Func<R, R, R> accumulate)
        {
            InitVal = initVal;
            Accumulate = accumulate;
            Handlers = new List<ICommand<T, R>>();
        }

        public void Add(ICommand<T, R> cmd)
        {
            Handlers.Add(cmd);
        }

        internal R Handle(Func<T> getArg)
        {
            if (Handlers.Count > 0)
            {
                var arg = getArg();
                return Handlers.Aggregate(InitVal, (acc, cmd) =>
                {
                    if (cmd.CanExecute(arg))
                        return Accumulate(acc, cmd.Execute(arg));
                    else
                        return acc;
                });
            }
            else
            {
                return InitVal;
            }
        }
    }
}
