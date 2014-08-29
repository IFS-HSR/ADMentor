using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EAAddInFramework
{
    public static class DiagramExtensions
    {
        public static bool Is(this EA.Diagram d, MDGBuilder.Diagram diagramType)
        {
            var match = Regex.Match(d.StyleEx, diagramStylePattern);

            return d.Type == diagramType.Type.Name && match.Success && match.Groups["diagramType"].Value == diagramType.Name;
        }

        private static string diagramStylePattern = @"MDGDgm=(?<technology>\w+)::(?<diagramType>\w+)";

        public static IEnumerable<EA.DiagramObject> DiagramObjects(this EA.Diagram d)
        {
            return d.DiagramObjects.Cast<EA.DiagramObject>();
        }
    }
}
