using Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Drawing;

namespace EAAddInFramework.MDGBuilder
{
    public interface IStereotype
    {
        String Name { get; }
        String DisplayName { get; }
        Enumeration Type { get; }
        XElement ToXml();
    }

    public class ElementStereotype : IStereotype
    {
        public ElementStereotype(String name, String displayName, ElementType type,
            Icon icon = null,
            String shapeScript = null,
            IEnumerable<TaggedValue> taggedValues = null,
            Color? backgroundColor = null,
            int? width = null,
            int? height = null,
            ElementStereotype instanceType = null)
        {
            Name = name;
            DisplayName = displayName;
            Type = type;
            Icon = icon.AsOption();
            ShapeScript = shapeScript.AsOption();
            TaggedValues = taggedValues ?? new TaggedValue[] { };
            BackgroundColor = backgroundColor.AsOption();
            Width = width.AsOption();
            Height = height.AsOption();
            InstanceType = instanceType.AsOption();
        }

        public string Name { get; private set; }

        public string DisplayName { get; private set; }

        public Enumeration Type { get; private set; }

        public Option<Icon> Icon { get; private set; }

        public Option<String> ShapeScript { get; private set; }

        public IEnumerable<TaggedValue> TaggedValues { get; private set; }

        public Option<Color> BackgroundColor { get; private set; }

        public Option<int> Width { get; private set; }

        public Option<int> Height { get; private set; }

        public Option<ElementStereotype> InstanceType { get; set; }

        public XElement ToXml()
        {
            return new XElement("Stereotype", new XAttribute("name", Name), new XAttribute("metatype", DisplayName),
                new XAttribute("instanceType", InstanceType.Select(it => it.Name).GetOrElse("")),
                new XAttribute("bgcolor", BackgroundColor.Select(bg => ToEaColor(bg)).GetOrElse("")),
                new XAttribute("cx", Width.Select(w => w.ToString()).GetOrElse("")),
                new XAttribute("cy", Height.Select(h => h.ToString()).GetOrElse("")),
                Icon.Select<Icon, XNode>(i => i.ToXml()).GetOrElse(new XComment("no custom icon")),
                ShapeScript.Select<String, XNode>(s => new ShapeScript(s).ToXml()).GetOrElse(new XComment("no custom shape")),
                new XElement("AppliesTo",
                    new XElement("Apply", new XAttribute("type", Type.ToString()))),
                new XElement("TaggedValues",
                    from tv in TaggedValues
                    select tv.ToXml()));
        }

        private static String ToEaColor(Color c)
        {
            return (((((int)c.B * 256) + c.G) * 256) + c.R).ToString();
        }
    }

    public class ElementType : Enumeration
    {
        public static readonly ElementType Action = new ElementType("Action");
        public static readonly ElementType Activity = new ElementType("Activity");
        public static readonly ElementType Event = new ElementType("Event");
        public static readonly ElementType Issue = new ElementType("Issue");

        private ElementType(String name) : base(name) { }
    }

    public static class BuiltInElementStereotypes
    {
        public static readonly ElementStereotype Issue = new ElementStereotype(name: "Functional", displayName: "Issue", type: ElementType.Issue);
    }
}
