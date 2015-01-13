using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInBase.MDGBuilder
{
    public sealed class DiagramType : Enumeration
    {
        public static readonly DiagramType Activity = new DiagramType(typeName: "Activity");

        public DiagramType(String typeName) : base(typeName) { }
    }
}
