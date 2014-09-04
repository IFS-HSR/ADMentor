using EAAddInFramework;
using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.ExportProblemSpace
{
    /// <summary>
    /// Common class hierarchy for EA model entities.
    /// Instances of ModelEntity are adapters for Elements, Connectors, Packages and Diagrams with a common interface for
    /// accessing data.
    /// </summary>
    public abstract class ModelEntity
    {
        private ModelEntity()
        {
            // just to seal the ModelEntity hierarchy
        }

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

        public Option<T> Match<T>() where T : ModelEntity
        {
            return Match(
                (T t) => Options.Some(t),
                () => Options.None<T>());
        }

        public String Name
        {
            get
            {
                return (this as dynamic).Val.Name as String;
            }
        }

        public String Stereotype
        {
            get
            {
                return Match(
                    (Package p) => p.Val.Element.Stereotype as String,
                    () => (this as dynamic).Val.Stereotype as String);
            }
        }

        public String Type
        {
            get
            {
                return Match(
                    (Package p) => p.Val.Element.Type as String,
                    () => (this as dynamic).Val.Type as String);
            }
        }

        public String MetaType
        {
            get
            {
                return Match(
                    (Package p) => p.Val.Element.MetaType as String,
                    () => (this as dynamic).Val.MetaType as String);
            }
        }

        public Option<String> Get(TaggedValue taggedValue)
        {
            return Match(
                (Package p) => p.Val.Element.Get(taggedValue),
                (Element e) => e.Val.Get(taggedValue),
                (Connector c) => { throw new NotImplementedException(); },
                (Diagram _) => Options.None<String>());
        }

        public IImmutableSet<String> Keywords
        {
            get
            {
                var element = Match(
                    (Package p) => p.Val.Element.AsOption(),
                    (Element e) => e.Val.AsOption(),
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
            public Package(EA.Package eaPackage)
            {
                Val = eaPackage;
            }

            public EA.Package Val { get; private set; }
        }

        public class Element : ModelEntity
        {
            public Element(EA.Element eaElement)
            {
                Val = eaElement;
            }

            public EA.Element Val { get; private set; }
        }

        public class Connector : ModelEntity
        {
            public Connector(EA.Connector eaConnector)
            {
                Val = eaConnector;
            }

            public EA.Connector Val { get; private set; }
        }

        public class Diagram : ModelEntity
        {
            public Diagram(EA.Diagram eaDiagram)
            {
                Val = eaDiagram;
            }

            public EA.Diagram Val { get; private set; }
        }
    }

    public static class ModelEntityExtensions
    {
        public static ModelEntity.Package AsModelEntity(this EA.Package p)
        {
            return new ModelEntity.Package(p);
        }

        public static ModelEntity.Element AsModelEntity(this EA.Element e)
        {
            return new ModelEntity.Element(e);
        }

        public static ModelEntity.Connector AsModelEntity(this EA.Connector c)
        {
            return new ModelEntity.Connector(c);
        }

        public static ModelEntity.Diagram AsModelEntity(this EA.Diagram d)
        {
            return new ModelEntity.Diagram(d);
        }
    }
}
