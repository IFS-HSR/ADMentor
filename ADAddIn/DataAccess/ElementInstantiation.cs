using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.DataAccess
{
    public class ElementInstantiation : IEquatable<ElementInstantiation>
    {
        public ElementInstantiation(EA.Element element, EA.Element instance, bool selected = false) : this(element, instance.AsOption(), selected) { }

        public ElementInstantiation(EA.Element element, Option<EA.Element> instance = null, bool selected = false)
        {
            Element = element;
            Instance = instance ?? Options.None<EA.Element>();
            Selected = selected;
        }

        public EA.Element Element { get; private set; }

        public Option<EA.Element> Instance { get; private set; }

        public bool Selected { get; private set; }

        public bool Equals(ElementInstantiation other)
        {
            return Element.ElementGUID == other.Element.ElementGUID
                && Instance.IsDefined == other.Instance.IsDefined
                && Instance.Match(e => e.ElementGUID == other.Instance.Value.ElementGUID, () => true)
                && Selected == other.Selected;
        }

        public ElementInstantiation Copy(EA.Element element = null, Option<EA.Element> instance = null, bool? selected = null)
        {
            return new ElementInstantiation(element ?? Element, instance ?? Instance, selected ?? Selected);
        }

        public ElementInstantiation CreateInstanceIfMissing(ElementRepository repo, EA.Package package)
        {
            return Instance.Match(
                _ => this,
                () => Copy(instance: repo.Instanciate(Element, package)));
        }
    }
}
