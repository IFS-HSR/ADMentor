using EAAddInFramework;
using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace EAAddInFramework.DataAccess
{
    /// <summary>
    /// Common class hierarchy for EA model entities.
    /// Instances of ModelEntity are adapters for Elements, Connectors, Packages and Diagrams with a common interface for
    /// accessing data.
    /// </summary>
    public abstract class ModelEntity
    {
        private ModelEntity(IEntityWrapper wrapper)
        {
            Wrapper = wrapper;
        }

        protected IEntityWrapper Wrapper { get; private set; }

        public R Match<T, R>(Func<T, R> onMatch, Func<R> otherwise) where T : ModelEntity
        {
            if (this is T)
                return onMatch(this as T);
            else
                return otherwise();
        }

        public R Match<T1, T2, R>(Func<T1, R> onMatch1, Func<T2, R> onMatch2, Func<R> otherwise)
            where T1 : ModelEntity
            where T2 : ModelEntity
        {
            return Match<T1, R>(
                e => onMatch1(e),
                () => Match<T2, R>(
                    e => onMatch2(e),
                    () => otherwise()));
        }

        public R Match<T1, T2, T3, T4, R>(Func<T1, R> onMatch1, Func<T2, R> onMatch2, Func<T3, R> onMatch3, Func<T4, R> onMatch4)
            where T1 : ModelEntity
            where T2 : ModelEntity
            where T3 : ModelEntity
            where T4 : ModelEntity
        {
            return Match<T1, R>(
                e => onMatch1(e),
                () => Match<T2, R>(
                    e => onMatch2(e),
                    () => Match<T3, R>(
                        e => onMatch3(e),
                        () => onMatch4(this as T4))));
        }

        public String Name
        {
            get
            {
                return (this as dynamic).EaObject.Name as String;
            }
        }

        public String Stereotype
        {
            get
            {
                return Match(
                    (Package p) => p.EaObject.Element.Stereotype as String,
                    () => (this as dynamic).EaObject.Stereotype as String);
            }
        }

        public String Type
        {
            get
            {
                return Match(
                    (Package p) => p.EaObject.Element.Type as String,
                    () => (this as dynamic).EaObject.Type as String);
            }
        }

        public String MetaType
        {
            get
            {
                return Match(
                    (Package p) => p.EaObject.Element.MetaType as String,
                    () => (this as dynamic).EaObject.MetaType as String);
            }
        }

        public Option<String> Get(TaggedValue taggedValue)
        {
            var taggedValues = Match(
                (Package p) => p.EaObject.Element.TaggedValues(),
                (Diagram d) => new List<EA.TaggedValue>().AsEnumerable(),
                (Element e) => e.EaObject.TaggedValues(),
                (Connector c) => c.EaObject.TaggedValues());
            return (from tv in taggedValues
                    where tv.Name.Equals(taggedValue.Name)
                    select tv.Value).FirstOption();
        }

        public IImmutableSet<String> Keywords
        {
            get
            {
                var element = Match(
                    (Package p) => p.EaObject.Element.AsOption(),
                    (Element e) => e.EaObject.AsOption(),
                    () => Options.None<EA.Element>());

                return (from e in element
                        from keyword in e.Tag
                            .Split(new[] { ',', ';' })
                            .Select(w => w.Trim().ToLower())
                        select keyword).ToImmutableHashSet();
            }
        }

        public class Package : ModelEntity
        {
            public Package(EA.Package eaPackage, IEntityWrapper wrapper)
                : base(wrapper)
            {
                EaObject = eaPackage;
            }

            public EA.Package EaObject { get; private set; }

            public Option<Package> GetParent(Func<int, Option<Package>> getPackageById)
            {
                return getPackageById(EaObject.ParentID);
            }

            public IEnumerable<Element> Elements()
            {
                return from e in EaObject.Elements.Cast<EA.Element>()
                       select Wrapper.Wrap(e);
            }

            public IEnumerable<Diagram> Diagrams()
            {
                return from d in EaObject.Diagrams.Cast<EA.Diagram>()
                       select Wrapper.Wrap(d);
            }

            public IEnumerable<Package> Packages()
            {
                return from p in EaObject.Packages.Cast<EA.Package>()
                       select Wrapper.Wrap(p);
            }

            public IEnumerable<Package> AllDescendants()
            {
                return from child in Packages()
                       from descendant in new List<Package> { child }.Concat(child.AllDescendants())
                       select descendant;
            }
        }

        public class Element : ModelEntity
        {
            public Element(EA.Element eaElement, IEntityWrapper wrapper)
                : base(wrapper)
            {
                EaObject = eaElement;
            }

            public EA.Element EaObject { get; private set; }

            public Option<ElementStereotype> GetStereotype(IEnumerable<ElementStereotype> stereotypes)
            {
                return (from stype in stereotypes
                        where EaObject.Is(stype)
                        select stype).FirstOption();
            }
        }

        public class Connector : ModelEntity
        {
            public Connector(EA.Connector eaConnector, IEntityWrapper wrapper)
                : base(wrapper)
            {
                EaObject = eaConnector;
            }

            public EA.Connector EaObject { get; private set; }
        }

        public class Diagram : ModelEntity
        {
            public Diagram(EA.Diagram eaDiagram, IEntityWrapper wrapper)
                : base(wrapper)
            {
                EaObject = eaDiagram;
            }

            public EA.Diagram EaObject { get; private set; }
        }
    }
}
