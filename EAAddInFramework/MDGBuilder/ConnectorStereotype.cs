using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utils;

namespace EAAddInFramework.MDGBuilder
{
    public class ConnectorStereotype : IStereotype
    {
        public ConnectorStereotype(
            String name,
            String displayName,
            ConnectorType type,
            String reverseDisplayName = null,
            IEnumerable<Connection> connects = null,
            Icon icon = null,
            String shapeScript = null,
            IEnumerable<TaggedValue> taggedValues = null,
            Direction direction = null,
            LineStyle lineStyle = null)
        {
            Name = name;
            DisplayName = displayName;
            ReverseDisplayName = reverseDisplayName.AsOption();
            Type = type;
            Connects = from c in connects ?? new Connection[] { }
                       select new Connection(c.From, c.To, this);
            Icon = icon.AsOption();
            ShapeScript = shapeScript.AsOption();
            TaggedValues = taggedValues ?? new TaggedValue[] { };
            Direction = direction.AsOption();
            LineStyle = lineStyle.AsOption();
        }

        public string Name { get; private set; }

        public string DisplayName { get; private set; }

        public Option<string> ReverseDisplayName { get; private set; }

        public Enumeration Type { get; private set; }

        public IEnumerable<Connection> Connects { get; private set; }

        public Option<Icon> Icon { get; private set; }

        public Option<String> ShapeScript { get; private set; }

        public IEnumerable<TaggedValue> TaggedValues { get; private set; }

        public Option<Direction> Direction { get; private set; }

        public Option<LineStyle> LineStyle { get; private set; }

        public XElement ToXml()
        {
            var taggedValues = LineStyle.Select(ls => TaggedValues.Concat(new TaggedValue[] {
                new TaggedValue(name: "_lineStyle", type: TaggedValueType.String.WithDefaultValue(ls.ToString()))
            })).GetOrElse(TaggedValues);

            return new XElement("Stereotype", new XAttribute("name", Name), new XAttribute("metatype", DisplayName),
                Icon.Select<Icon, XNode>(i => i.ToXml()).GetOrElse(new XComment("no custom icon")),
                ShapeScript.Select<String, XNode>(s => new ShapeScript(s).ToXml()).GetOrElse(new XComment("no custom shape")),
                new XElement("AppliesTo",
                    new XElement("Apply", new XAttribute("type", Type.ToString()),
                        new XElement("Property", new XAttribute("name", "direction"), new XAttribute("value", Direction.Select(d => d.Name).GetOrElse(""))))),
                new XElement("TaggedValues",
                    from tv in taggedValues
                    select tv.ToXml()));
        }
    }

    public class Connection
    {
        public Connection(ElementStereotype from, ElementStereotype to) : this(from, to, null) { }

        internal Connection(ElementStereotype from, ElementStereotype to, ConnectorStereotype connectorStereotype)
        {
            From = from;
            To = to;
            ConnectorStereotype = connectorStereotype;
        }

        public ElementStereotype From { get; private set; }

        public ElementStereotype To { get; private set; }

        public ConnectorStereotype ConnectorStereotype { get; private set; }

        /// <summary>
        /// Generates quick links in EA's quick linker definition format
        /// http://www.sparxsystems.com/enterprise_architect_user_guide/10/extending_uml_models/quick_linker_definition_format.html
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<String> GetQuickLinkEntries()
        {
            return new String[] {
                MakeToExistingElementQuickLink(),
                MakeFromExistingElementQuickLink(),
                MakeNewTargetElementQuickLink(),
                MakeNewSourceElementQuickLink()
            };
        }

        private String MakeToExistingElementQuickLink()
        {
            return String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},",
                /* A */From.Type.Name,
                /* B */From.Name,
                /* C */To.Type.Name,
                /* D */To.Name,
                /* E */"",
                /* F */"",
                /* G */"",
                /* H */ConnectorStereotype.Type.Name,
                /* I */ConnectorStereotype.Name,
                /* J */"to",
                /* K */ConnectorStereotype.DisplayName,
                /* L */"",
                /* M */"TRUE",
                /* N */"",
                /* O */"",
                /* P */"TRUE",
                /* Q */"",
                /* R */"0",
                /* S */"",
                /* T */"",
                /* U */"",
                /* V */"");
        }

        private String MakeFromExistingElementQuickLink()
        {
            return String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},",
                /* A */To.Type.Name,
                /* B */To.Name,
                /* C */From.Type.Name,
                /* D */From.Name,
                /* E */"",
                /* F */"",
                /* G */"",
                /* H */ConnectorStereotype.Type.Name,
                /* I */ConnectorStereotype.Name,
                /* J */"from",
                /* K */ConnectorStereotype.ReverseDisplayName.GetOrElse(ConnectorStereotype.DisplayName),
                /* L */"",
                /* M */"TRUE",
                /* N */"",
                /* O */"",
                /* P */"TRUE",
                /* Q */"",
                /* R */"0",
                /* S */"",
                /* T */"",
                /* U */"",
                /* V */"");
        }

        private String MakeNewTargetElementQuickLink()
        {
            return String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},",
                /* A */From.Type.Name,
                /* B */From.Name,
                /* C */"",
                /* D */"",
                /* E */"",
                /* F */To.Type.Name,
                /* G */To.Name,
                /* H */ConnectorStereotype.Type.Name,
                /* I */ConnectorStereotype.Name,
                /* J */"to",
                /* K */"",
                /* L */To.DisplayName,
                /* M */"TRUE",
                /* N */"TRUE",
                /* O */"",
                /* P */"TRUE",
                /* Q */ConnectorStereotype.DisplayName,
                /* R */"0",
                /* S */"",
                /* T */"",
                /* U */"",
                /* V */"");
        }

        private String MakeNewSourceElementQuickLink()
        {
            return String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},",
                /* A */To.Type.Name,
                /* B */To.Name,
                /* C */"",
                /* D */"",
                /* E */"",
                /* F */From.Type.Name,
                /* G */From.Name,
                /* H */ConnectorStereotype.Type.Name,
                /* I */ConnectorStereotype.Name,
                /* J */"from",
                /* K */"",
                /* L */From.DisplayName,
                /* M */"TRUE",
                /* N */"TRUE",
                /* O */"",
                /* P */"TRUE",
                /* Q */ConnectorStereotype.ReverseDisplayName.GetOrElse(ConnectorStereotype.DisplayName),
                /* R */"0",
                /* S */"",
                /* T */"",
                /* U */"",
                /* V */"");
        }
    }

    public class ConnectorType : Enumeration
    {
        public static readonly ConnectorType Aggregation = new ConnectorType("Aggregation", Direction.SourceToDestination);
        public static readonly ConnectorType Association = new ConnectorType("Association", Direction.Unspecified);
        public static readonly ConnectorType Dependency = new ConnectorType("Dependency", Direction.SourceToDestination);

        private ConnectorType(string name, Direction defaultDirection)
            : base(name)
        {
            DefaultDirecttion = defaultDirection;
        }

        public Direction DefaultDirecttion { get; private set; }
    }

    public sealed class Direction : Enumeration
    {
        public static readonly Direction Unspecified = new Direction("Unspecified");
        public static readonly Direction SourceToDestination = new Direction("Source -> Destination");
        public static readonly Direction DestinationToSource = new Direction("Destination -> Source");
        public static readonly Direction BiDirectional = new Direction("Bi-Directional");

        private Direction(String name) : base(name) { }
    }

    public sealed class LineStyle : Enumeration
    {
        public static readonly LineStyle Auto = new LineStyle("auto");
        public static readonly LineStyle TreeLateralHorizontal = new LineStyle("treeLV");
        public static readonly LineStyle TreeLateralVertical = new LineStyle("treeLH");

        private LineStyle(String name) : base(name) { }
    }
}
