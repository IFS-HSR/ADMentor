using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace EAAddInFramework
{
    public class LoggedAtom<T> : Atom<T>
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public LoggedAtom(String name, T init)
            : base(init)
        {
            Name = name;
        }

        public override void Exchange(T v, Type sender)
        {
            logger.Debug("Set atom {0} to {1} (by {2})", Name, v, sender.FullName);
            base.Exchange(v, sender);
        }

        public string Name { get; private set; }
    }
}
