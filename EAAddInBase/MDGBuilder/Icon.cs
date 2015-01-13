using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EAAddInBase.MDGBuilder
{
    public class Icon
    {
        public Icon(string resourceName)
        {
            Assembly = Assembly.GetCallingAssembly();
            ResourceName = resourceName;
        }

        public string ResourceName { get; private set; }

        private Assembly Assembly { get; set; }

        public XElement ToXml()
        {
            var dt = XNamespace.Get("urn:schemas-microsoft-com:datatypes");
            return new XElement("Icon",
                new XAttribute("type", "bitmap"),
                new XAttribute(XNamespace.Xmlns + "dt", "urn:schemas-microsoft-com:datatypes"),
                new XAttribute(dt + "dt", "bin.base64"),
                EAAddInBase.Utils.Resources.GetBase64Encoded(Assembly, ResourceName));
        }
    }
}
