using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInFramework
{
    public static class DiagramExtensions
    {
        public static IEnumerable<EA.DiagramObject> DiagramObjects(this EA.Diagram d)
        {
            return d.DiagramObjects.Cast<EA.DiagramObject>();
        }
    }
}
