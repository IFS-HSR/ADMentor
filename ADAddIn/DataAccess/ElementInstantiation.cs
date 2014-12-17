using EAAddInFramework.DataAccess;
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
        public ElementInstantiation(ModelEntity.Element element, Option<ModelEntity.Element> instance = null, bool selected = false)
        {
            Element = element;
            Instance = instance ?? Options.None<ModelEntity.Element>();
            Selected = selected;
        }

        public ModelEntity.Element Element { get; private set; }

        public Option<ModelEntity.Element> Instance { get; private set; }

        public bool Selected { get; private set; }

        public bool Equals(ElementInstantiation other)
        {
            return Element.Equals(other.Element)
                && Instance.IsDefined == other.Instance.IsDefined
                && Instance.Fold(e => e.Equals(other.Instance.Value), () => true)
                && Selected == other.Selected;
        }

        public ElementInstantiation Copy(ModelEntity.Element element = null, Option<ModelEntity.Element> instance = null, bool? selected = null)
        {
            return new ElementInstantiation(element ?? Element, instance ?? Instance, selected ?? Selected);
        }

        public ElementInstantiation CreateInstanceIfMissing(ModelEntityRepository repo, ModelEntity.Package package)
        {
            return Instance.Fold(
                _ => this,
                () => Copy(instance: repo.Instanciate(Element, package, ADTechnology.Technologies.AD.ElementStereotypes)));
        }
    }
}
