using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInFramework
{
    public static class ElementExtensions
    {
        public static bool Is(this EA.Element e, ElementStereotype stereotype)
        {
            return e.Stereotype == stereotype.Name && e.Type == stereotype.Type.Name;
        }

        public static bool IsInstance(this EA.Element e)
        {
            return e.ClassifierID != 0;
        }
    }
}
