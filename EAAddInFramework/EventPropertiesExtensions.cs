using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInFramework
{
    public static class EventPropertiesExtensions
    {
        public static int ExtractElementId(this EA.EventProperties info)
        {
            return Int32.Parse(info.Get("ElementId").Value.ToString());
        }

        public static string ExtractType(this EA.EventProperties info)
        {
            return info.Get("Type").Value.ToString();
        }

        public static string ExtractStereotype(this EA.EventProperties info)
        {
            return info.Get("Stereotype").Value.ToString();
        }

        public static int ExtractParentId(this EA.EventProperties info)
        {
            return Int32.Parse(info.Get("ParentId").Value.ToString());
        }

        public static int ExtractDiagramId(this EA.EventProperties info)
        {
            return Int32.Parse(info.Get("DiagramId").Value.ToString());
        }
    }
}
