using EAAddInBase.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Drawing;

namespace EAAddInBase.MDGBuilder
{
    public abstract class Stereotype
    {
        public abstract String Name { get; protected set; }
        public abstract String DisplayName { get; protected set; }
        public abstract Enumeration Type { get; protected set; }
        public abstract IEnumerable<TaggedValueDefinition> TaggedValues { get; protected set; }
        internal abstract XElement ToXml(TaggedValueDefinition versionTag);
    }

    public sealed class ElementStereotype : Stereotype
    {
        public ElementStereotype(String name, String displayName, ElementType type,
            Icon icon = null,
            String shapeScript = null,
            IEnumerable<TaggedValueDefinition> taggedValues = null,
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
            TaggedValues = taggedValues ?? new TaggedValueDefinition[] { };
            BackgroundColor = backgroundColor.AsOption();
            Width = width.AsOption();
            Height = height.AsOption();
            InstanceType = instanceType.AsOption();
        }

        public override string Name { get; protected set; }

        public override string DisplayName { get; protected set; }

        public override Enumeration Type { get; protected set; }

        public Option<Icon> Icon { get; private set; }

        public Option<String> ShapeScript { get; private set; }

        public override IEnumerable<TaggedValueDefinition> TaggedValues { get; protected set; }

        public Option<Color> BackgroundColor { get; private set; }

        public Option<int> Width { get; private set; }

        public Option<int> Height { get; private set; }

        public Option<ElementStereotype> InstanceType { get; set; }

        internal override XElement ToXml(TaggedValueDefinition versionTag)
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
                    from tv in TaggedValues.Concat(new[] { versionTag })
                    select tv.ToXml()));
        }

        private static String ToEaColor(Color c)
        {
            // EA uses the same color codes as MS Access
            // see also http://stackoverflow.com/questions/16198122/convert-ms-access-color-code-to-hex-in-c-sharp
            return (((((int)c.B * 256) + c.G) * 256) + c.R).ToString();
        }
    }

    public sealed class PackageStereotype
    {
        public PackageStereotype(String name, String displayName, Icon icon = null, IEnumerable<TaggedValueDefinition> taggedValues = null,
            Color? backgroundColor = null) {
                Element = new ElementStereotype(
                    name: name, 
                    displayName: displayName, 
                    type: ElementType.Package, 
                    icon: icon, 
                    taggedValues: taggedValues,
                    backgroundColor: backgroundColor);
        }

        public ElementStereotype Element { get; private set; }
    }

    public sealed class ElementType : Enumeration
    {
        public static readonly ElementType Action = new ElementType("Action");
        public static readonly ElementType Activity = new ElementType("Activity");
        public static readonly ElementType Actor = new ElementType("Actor");
        public static readonly ElementType Class = new ElementType("Class");
        public static readonly ElementType Event = new ElementType("Event");
        public static readonly ElementType Issue = new ElementType("Issue");
        public static readonly ElementType Object = new ElementType("Object");
        public static readonly ElementType Package = new ElementType("Package");
        public static readonly ElementType Requirement = new ElementType("Requirement");

        private ElementType(String name)
            : base(name)
        {
            DefaultStereotype = new ElementStereotype(name: "", displayName: Name, type: this);
        }

        public ElementStereotype DefaultStereotype { get; private set; }
    }
}
