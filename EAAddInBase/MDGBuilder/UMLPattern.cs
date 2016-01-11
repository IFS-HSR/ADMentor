using EAAddInBase.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace EAAddInBase.MDGBuilder
{
    public sealed class UMLPattern
    {
        public UMLPattern(string displayName, string resourceName)
        {
            DisplayName = displayName;
            ResourceName = resourceName;
            document = XDocument.Parse(Resources.GetAsString(Assembly.GetCallingAssembly(), ResourceName));
        }
        
        public string DisplayName { get; private set; }

        public string ResourceName { get; private set; }

        public string Name
        {
            get
            {
                var metadata = document.XPathSelectElement("//UMLPattern");
                return metadata.Attribute("name").Value;
            }
        }

        private readonly XDocument document;

        internal XElement ToXml()
        {
            return new XElement("UMLPattern", document.Elements());
        }
    }
}
