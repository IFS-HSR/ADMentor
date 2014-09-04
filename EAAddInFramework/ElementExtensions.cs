using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

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

        public static bool IsNew(this EA.Element e)
        {
            return DateTime.Now - e.Created < TimeSpan.FromSeconds(1);
        }

        public static EA.Package FindPackage(this EA.Element element, EA.Repository repo)
        {
            return repo.AllPackages().First(
                pkg => pkg.Elements.Cast<EA.Element>().Any(
                    e => e.ElementID == element.ElementID));
        }

        public static IEnumerable<EA.Connector> Connectors(this EA.Element e)
        {
            return e.Connectors.Cast<EA.Connector>();
        }

        public static Option<String> Get(this EA.Element e, TaggedValue taggedValue)
        {
            return (from tv in e.TaggedValues.Cast<EA.TaggedValue>()
                    where tv.Name == taggedValue.Name
                    where tv.Value != ""
                    select tv.Value).FirstOption();
        }

        public static void Set(this EA.Element e, TaggedValue taggedValue, String value)
        {
            (from tv in e.TaggedValues.Cast<EA.TaggedValue>()
             where tv.Name == taggedValue.Name
             select tv).FirstOption()
                .Match(
                tv =>
                {
                    tv.Value = value;
                    tv.Update();
                    return Unit.Instance;
                },
                () =>
                {
                    var tv = e.TaggedValues.AddNew(taggedValue.Name, "") as EA.TaggedValue;
                    tv.Value = value;
                    tv.Update();
                    e.TaggedValues.Refresh();
                    return Unit.Instance;
                });
        }
    }
}
