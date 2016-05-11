using EAAddInBase;
using EAAddInBase.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace EAAddInBase.DataAccess
{
    /// <summary>
    /// Common class hierarchy for EA model entities.
    /// Instances of ModelEntity are adapters for Elements, Connectors, Packages and Diagrams with a common interface for
    /// accessing data.
    /// </summary>
    public abstract class ModelEntity : IEquatable<ModelEntity>
    {
        private ModelEntity(IEntityWrapper wrapper)
        {
            Wrapper = wrapper;
        }

        protected IEntityWrapper Wrapper { get; private set; }

        public int Id
        {
            get
            {
                return this.Match<ModelEntity, int>()
                    .Case<Package>(p => p.EaObject.PackageID)
                    .Case<Diagram>(d => d.EaObject.DiagramID)
                    .Case<Element>(e => e.EaObject.ElementID)
                    .Case<Connector>(c => c.EaObject.ConnectorID)
                    .GetOrThrowNotImplemented();
            }
        }

        public String Guid
        {
            get
            {
                return this.Match<ModelEntity, String>()
                    .Case<Package>(p => p.EaObject.PackageGUID)
                    .Case<Diagram>(d => d.EaObject.DiagramGUID)
                    .Case<Element>(e => e.EaObject.ElementGUID)
                    .Case<Connector>(c => c.EaObject.ConnectorGUID)
                    .GetOrThrowNotImplemented();
            }
        }

        public override bool Equals(object obj)
        {
            return obj.TryCast<ModelEntity>()
                .Select(otherEntity => Equals(otherEntity))
                .GetOrElse(false);
        }

        public bool Equals(ModelEntity other)
        {
            return Guid.Equals(other.Guid);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
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
                return this.Match<ModelEntity, String>()
                    .Case<Package>(p => p.AssociatedElement.Select(e => e.Stereotype).GetOrElse(""))
                    .Default(entity => (entity as dynamic).EaObject.Stereotype as String);
            }
        }

        public String Type
        {
            get
            {
                return this.Match<ModelEntity, String>()
                    .Case<Package>(p => p.AssociatedElement.Select(e => e.Type).GetOrElse(""))
                    .Default(entity => (entity as dynamic).EaObject.Type as String);
            }
        }

        public String MetaType
        {
            get
            {
                return this.Match<ModelEntity, String>()
                    .Case<Package>(p => p.AssociatedElement.Select(e => e.MetaType).GetOrElse(""))
                    .Default(entity => (entity as dynamic).EaObject.MetaType as String);
            }
        }

        public String EntityType
        {
            get
            {
                return this.Match<ModelEntity, String>()
                    .Case<Package>(_ => "Package")
                    .Case<Diagram>(_ => "Diagram")
                    .Case<Element>(_ => "Element")
                    .Case<Connector>(_ => "Connector")
                    .GetOrThrowNotImplemented();
            }
        }

        public Option<Package> GetParent(Func<int, Option<Package>> getPackageById)
        {
            return from id in this.Match<ModelEntity, Option<int>>()
                        .Case<Package>(p => p.EaObject.ParentID.AsOption())
                        .Case<Diagram>(d => d.EaObject.PackageID.AsOption())
                        .Case<Element>(e => e.EaObject.PackageID.AsOption())
                        .Case<Connector>(_ => Options.None<int>())
                        .GetOrThrowNotImplemented()
                   from p in getPackageById(id)
                   select p;
        }

        public IEnumerable<String> GetPath(Func<int, Option<Package>> getPackageById)
        {
            return GetParent(getPackageById).Fold(
                p => p.GetPath(getPackageById).Concat(new[] { Name }),
                () => new[] { Name });
        }

        private Option<EA.Collection> TaggedValuesCollection
        {
            get
            {
                return this.Match<ModelEntity, Option<EA.Collection>>()
                    .Case<Package>(p => from e in p.AssociatedElement
                                        from c in e.TaggedValuesCollection
                                        select c)
                    .Case<Diagram>(d => Options.None<EA.Collection>())
                    .Case<Element>(e => Options.Some(e.EaObject.TaggedValues))
                    .Case<Connector>(c => Options.Some(c.EaObject.TaggedValues))
                    .GetOrThrowNotImplemented();
            }
        }

        public ImmutableDictionary<String, String> TaggedValues
        {
            get
            {
                // All tagged values are equal but some are more equal (ConnectorTags)
                // thats why we have to use dynamic for working with tagged values in a generic way
                return TaggedValuesCollection
                    .Select(tvc => tvc.Cast<dynamic>())
                    .GetOrElse(Enumerable.Empty<dynamic>())
                    // in some cases it is possible that an element has multiple tagged values with the same name
                    .DistinctBy(tv => tv.Name)
                    .ToImmutableDictionary(tv => tv.Name as String, tv => tv.Value as String);
            }
        }

        public Option<String> Get(TaggedValue taggedValue)
        {
            return (from tvc in TaggedValuesCollection
                    from tv in tvc.Cast<dynamic>()
                    let name = tv.Name as String
                    where name.Equals(taggedValue.Name)
                    select tv.Value as String).FirstOption();
        }

        public void Set(TaggedValue tv, String value)
        {
            Set(tv.Name, value);
        }

        public void Set(String name, String value)
        {
            TaggedValuesCollection
                .Do(taggedValues =>
                {
                    // All tagged values are equal but some are more equal (ConnectorTags)
                    // thats why we have to use dynamic for working with tagged values in a generic way
                    (from tv in taggedValues.Cast<dynamic>()
                     where tv.Name.Equals(name)
                     select tv)
                    .FirstOption()
                    .Fold(
                        tv =>
                        {
                            tv.Value = value;
                            tv.Update();
                        },
                        () =>
                        {
                            var tv = taggedValues.AddNew(name, "") as dynamic;

                            tv.Value = value;
                            tv.Update();
                            taggedValues.Refresh();
                        });
                });
        }

        public IImmutableSet<String> Keywords
        {
            get
            {
                var element = this.Match<ModelEntity, Option<EA.Element>>()
                    .Case<Package>(p => p.EaObject.Element.AsOption())
                    .Case<Element>(e => e.EaObject.AsOption())
                    .Default(_ => Options.None<EA.Element>());

                return (from e in element
                        from keyword in e.Tag
                            .Split(new[] { ',', ';' })
                            .Select(w => w.Trim().ToLower())
                        select keyword).ToImmutableHashSet();
            }
        }

        public Option<String> Author
        {
            get
            {
                return this.Match<ModelEntity, Option<String>>()
                    .Case<Package>(p => p.EaObject.Element.AsOption().Select(e => e.Author))
                    .Default(_ => Options.Some((this as dynamic).EaObject.Author as String));
            }
        }

        public Option<String> Status
        {
            get
            {
                return this.Match<ModelEntity, Option<String>>()
                    .Case<Diagram>(_ => Options.None<String>())
                    .Case<Package>(p => p.EaObject.Element.AsOption().Select(e => e.Status))
                    .Default(_ => Options.Some((this as dynamic).EaObject.Status as String));
            }
        }

        public string Version
        {
            get
            {
                return (this as dynamic).EaObject.Version as String;
            }
        }

        public String Notes
        {
            get
            {
                return (this as dynamic).EaObject.Notes as String;
            }
        }

        public bool Is(Stereotype stype)
        {
            return Stereotype.Equals(stype.Name) && Type.Equals(stype.Type.Name);
        }

        public override string ToString()
        {
            var stereotype = MetaType == "" ? "" : String.Format("<<{0}>> ", MetaType);
            return String.Format("{0}: {1}{2}", EntityType, stereotype, Name);
        }

        public class Package : ModelEntity
        {
            public Package(EA.Package eaPackage, IEntityWrapper wrapper)
                : base(wrapper)
            {
                EaObject = eaPackage;
            }

            public EA.Package EaObject { get; private set; }

            public Option<Element> AssociatedElement
            {
                get
                {
                    // root models do not have an associated element!
                    return from e in EaObject.Element.AsOption()
                           select Wrapper.Wrap(e);
                }
            }

            public IEnumerable<Element> Elements
            {
                get
                {
                    return from e in EaObject.Elements.Cast<EA.Element>()
                           select Wrapper.Wrap(e);
                }
            }

            public IEnumerable<Diagram> Diagrams
            {
                get
                {
                    return from d in EaObject.Diagrams.Cast<EA.Diagram>()
                           select Wrapper.Wrap(d);
                }
            }

            /// <summary>
            /// All children of this package.
            /// </summary>
            /// <returns></returns>
            public IEnumerable<Package> Packages
            {
                get
                {
                    return from p in EaObject.Packages.Cast<EA.Package>()
                           select Wrapper.Wrap(p);
                }
            }

            /// <summary>
            /// This package with all packages that are descendant of this package.
            /// </summary>
            /// <returns></returns>
            public IEnumerable<Package> SubPackages
            {
                get
                {
                    return new[] { this }.Concat(
                        from child in Packages
                        from descendant in child.SubPackages
                        select descendant);
                }
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
                        where Is(stype)
                        select stype).FirstOption();
            }

            public Option<Element> GetClassifier(Func<int, Option<Element>> getElementById)
            {
                return getElementById(EaObject.ClassifierID);
            }

            public IEnumerable<Connector> Connectors
            {
                get
                {
                    return from c in EaObject.Connectors.Cast<EA.Connector>()
                           select Wrapper.Wrap(c);
                }
            }

            public bool IsNew()
            {
                return DateTime.Now - EaObject.Created < TimeSpan.FromSeconds(1);
            }

            public IEnumerable<Element> ElementsConnectedBy(ConnectorStereotype cstype, Func<int, Option<ModelEntity.Element>> getElementById)
            {
                return from connector in Connectors
                       where connector.Is(cstype)
                       from source in connector.OppositeEnd(this, getElementById)
                       select source;
            }

            public IEnumerable<E> ElementsConnectedBy<E>(ConnectorStereotype cstype, Func<int, Option<ModelEntity.Element>> getElementById)
                where E : Element
            {
                return from element in ElementsConnectedBy(cstype, getElementById)
                       from e in element.TryCast<E>()
                       select e;
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

            public Option<ConnectorStereotype> GetStereotype(IEnumerable<ConnectorStereotype> stereotypes)
            {
                return (from stype in stereotypes
                        where Is(stype)
                        select stype).FirstOption();
            }

            public ConnectorStereotype ReconstructStereotype()
            {
                return new ConnectorStereotype(
                           name: EaObject.Stereotype,
                           displayName: EaObject.Stereotype,
                           type: GetCType());
            }

            public ConnectorType GetCType()
            {
                return ConnectorType.All
                    .FirstOption(t => t.Name == EaObject.Type)
                    .GetOrElse(() => { throw new ApplicationException("Connector type " + EaObject.Type + " not found"); });
            }

            public Option<Element> Source(Func<int, Option<Element>> getElementById)
            {
                return getElementById(EaObject.ClientID);
            }

            public Option<Element> Target(Func<int, Option<Element>> getElementById)
            {
                return getElementById(EaObject.SupplierID);
            }

            public Option<Element> OppositeEnd(Element thisEnd, Func<int, Option<Element>> getElementById)
            {
                if (EaObject.ClientID == thisEnd.Id)
                    return getElementById(EaObject.SupplierID);
                else if (EaObject.SupplierID == thisEnd.Id)
                    return getElementById(EaObject.ClientID);
                else return Options.None<Element>();
            }

            public bool Is(ConnectorStereotype connectorStereotype)
            {
                return Stereotype.Equals(connectorStereotype.Name) && Type.Equals(connectorStereotype.Type.Name);
            }
        }

        public class Diagram : ModelEntity
        {
            public Diagram(EA.Diagram eaDiagram, IEntityWrapper wrapper)
                : base(wrapper)
            {
                EaObject = eaDiagram;
            }

            public EA.Diagram EaObject { get; private set; }

            public bool Is(MDGBuilder.Diagram diagramType)
            {
                var match = Regex.Match(EaObject.StyleEx, diagramStylePattern);

                return EaObject.Type.Equals(diagramType.Type.Name) && match.Success && match.Groups["diagramType"].Value.Equals(diagramType.Name);
            }

            private static string diagramStylePattern = @"MDGDgm=(?<technology>\w+)::(?<diagramType>\w+)";

            public IEnumerable<DiagramObject> Objects
            {
                get
                {
                    return from o in EaObject.DiagramObjects.Cast<EA.DiagramObject>()
                           select Wrapper.Wrap(o);
                }
            }

            public Option<DiagramObject> GetObject(Element forElement)
            {
                return (from o in Objects
                        where o.EaObject.ElementID.Equals(forElement.Id)
                        select o).FirstOption();
            }

            public DiagramObject AddObject(int elementId, int left, int right, int top, int bottom)
            {
                var pos = String.Format("l={0};r={1};t={2};b={3};", left, right, top, bottom);
                var obj = EaObject.DiagramObjects.AddNew(pos, "") as EA.DiagramObject;
                obj.ElementID = elementId;
                obj.Update();
                EaObject.DiagramObjects.Refresh();

                return Wrapper.Wrap(obj);
            }

            public IEnumerable<ModelEntity> SelectedEntities(ModelEntityRepository repo)
            {
                return EaObject.SelectedObjects.Cast<EA.DiagramObject>()
                    .SelectMany(obj => repo.GetElement(obj.ElementID))
                    .Concat<ModelEntity>(
                        EaObject.SelectedConnector.AsOption().Select(c => Wrapper.Wrap(c)));
            }
        }
    }
}
