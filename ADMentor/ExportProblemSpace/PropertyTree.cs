using EAAddInBase.DataAccess;
using EAAddInBase.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace ADMentor.ExportProblemSpace
{
    public class PropertyTree
    {
        public PropertyTree(ModelEntity entity, IImmutableSet<Tuple<String, Option<String>>> properties, IImmutableList<PropertyTree> children)
        {
            Entity = entity;
            Properties = properties;
            Children = children;
        }

        public ModelEntity Entity { get; private set; }

        public IImmutableSet<Tuple<String, Option<String>>> Properties { get; private set; }

        public IImmutableList<PropertyTree> Children { get; private set; }

        public PropertyTree ApplyFilter(IFilter<PropertyTree> f)
        {
            return new PropertyTree(
                Entity, Properties, Children.Where(child => f.Accept(child)).Select(child => child.ApplyFilter(f)).ToImmutableList());
        }

        public static PropertyTree Create(ModelEntity e, Func<ModelEntity, IDictionary<String, IEnumerable<Option<String>>>> propertiesGetter)
        {
            var pt = Create(e, me =>
            {
                var properties = propertiesGetter(me);

                return (from property in properties
                        from value in property.Value
                        select Tuple.Create(property.Key, value)).ToImmutableHashSet();
            });

            return DeriveDiagramProperties(pt, pt);
        }

        private static PropertyTree Create(ModelEntity e, Func<ModelEntity, IImmutableSet<Tuple<String, Option<String>>>> getProperties)
        {
            var children = e.Match<ModelEntity, IEnumerable<PropertyTree>>()
                .Case<ModelEntity.Package>(p =>
                {
                    return from entity in p.Elements.Concat<ModelEntity>(p.Diagrams).Concat(p.Packages)
                           select Create(entity, getProperties);
                })
                .Default(_ => Enumerable.Empty<PropertyTree>()).ToImmutableList();

            return UpPropagateProperties(new PropertyTree(e, getProperties(e), children));
        }

        private static PropertyTree UpPropagateProperties(PropertyTree propertyTree)
        {
            var props = propertyTree.Children.Aggregate(propertyTree.Properties, (ps, t) => ps.Union(t.Properties));
            return new PropertyTree(propertyTree.Entity, props, propertyTree.Children);
        }

        private static PropertyTree DeriveDiagramProperties(PropertyTree root, PropertyTree pt)
        {
            return pt.Entity.Match<ModelEntity, PropertyTree>()
                .Case<ModelEntity.Diagram>(d =>
                {
                    var elementIds = d.Objects.Select(obj => obj.EaObject.ElementID).Run();
                    var properties = CollectProperties(root, elementIds);
                    return new PropertyTree(pt.Entity, properties, pt.Children);
                })
                .Default(_ => new PropertyTree(pt.Entity, pt.Properties, pt.Children.Select(child => DeriveDiagramProperties(root, child)).ToImmutableList()));
        }

        private static IImmutableSet<Tuple<String, Option<String>>> CollectProperties(PropertyTree pt, IEnumerable<int> elementIds)
        {
            var elementId = pt.Entity.Match<ModelEntity, Option<int>>()
                .Case<ModelEntity.Package>(p => p.AssociatedElement.Select(e => e.Id))
                .Default(e => e.Id.AsOption());

            return (from eid in elementId
                    where elementIds.Contains(eid)
                    select pt.Properties)
                    .GetOrElse(() =>
                    {
                        return pt.Children.Aggregate(
                            ImmutableHashSet.Create<Tuple<String, Option<String>>>(),
                            (props, t) => props.Union(CollectProperties(t, elementIds)));
                    });
        }

        public IEnumerable<ModelEntity> AllEntities()
        {
            return (from child in Children
                    from entity in child.AllEntities()
                    select entity).Concat(new[] { Entity });
        }
    }
}
