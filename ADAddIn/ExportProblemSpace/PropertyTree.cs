using EAAddInFramework.DataAccess;
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
    public class PropertyTree
    {
        public PropertyTree(ModelEntity entity, ILookup<String, String> properties, IEnumerable<PropertyTree> children)
        {
            Entity = entity;
            Properties = properties;
            Children = children;
        }

        public ModelEntity Entity { get; private set; }

        public ILookup<String, String> Properties { get; private set; }

        public IEnumerable<PropertyTree> Children { get; private set; }

        public PropertyTree ApplyFilter(IFilter<PropertyTree> f)
        {
            return new PropertyTree(
                Entity, Properties, Children.Where(child => f.Accept(child)).Select(child => child.ApplyFilter(f)));
        }

        public static PropertyTree Create(ModelEntity e, Func<ModelEntity, IDictionary<String,IEnumerable<String>>> propertiesGetter)
        {
            var pt = Create(e, me =>
            {
                var properties = propertiesGetter(me);

                return (from property in properties
                        from value in property.Value
                        select Tuple.Create(property.Key, value)).ToLookup(kv => kv.Item1, kv => kv.Item2);
            });

            return DeriveDiagramProperties(pt, pt);
        }

        private static PropertyTree Create(ModelEntity e, Func<ModelEntity, ILookup<String, String>> getProperties)
        {
            var children = e.Match<ModelEntity, IEnumerable<PropertyTree>>()
                .Case<ModelEntity.Package>(p =>
                {
                    return from entity in p.Elements.Concat<ModelEntity>(p.Diagrams).Concat(p.Packages)
                           select Create(entity, getProperties);
                })
                .Default(_ => Enumerable.Empty<PropertyTree>()).Run();

            return UpPropagateProperties(new PropertyTree(e, getProperties(e), children));
        }

        private static PropertyTree UpPropagateProperties(PropertyTree propertyTree)
        {
            var props = propertyTree.Children.Aggregate(propertyTree.Properties, (ps, t) => ps.Merge(t.Properties));
            return new PropertyTree(propertyTree.Entity, props, propertyTree.Children);
        }

        private static PropertyTree DeriveDiagramProperties(PropertyTree root, PropertyTree pt)
        {
            return pt.Entity.Match<ModelEntity, PropertyTree>()
                .Case<ModelEntity.Diagram>(d =>{
                    var elementIds = d.Objects.Select(obj => obj.EaObject.ElementID);
                    var properties = CollectDiagramProperties(root, elementIds);
                    return new PropertyTree(pt.Entity, properties, pt.Children);
                })
                .Default(_ => new PropertyTree(pt.Entity, pt.Properties, pt.Children.Select(child => DeriveDiagramProperties(root, child))));
        }

        private static ILookup<String, String> CollectDiagramProperties(PropertyTree pt, IEnumerable<int> elementIds)
        {
            if (elementIds.Contains(pt.Entity.Id))
            {
                return pt.Properties;
            }
            else
            {
                var emptyLookup = ImmutableDictionary.Create<String, String>().ToLookup(k => k.Key, v => v.Value);
                return pt.Children.Aggregate(emptyLookup, (props, t) => props.Merge(CollectDiagramProperties(t, elementIds)));
            }
        }
    }
}
