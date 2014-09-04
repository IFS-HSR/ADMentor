using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
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
    public class ExportProblemSpaceCommand : ICommand<EA.Package, Unit>
    {
        private readonly ElementRepository ElementRepo;
        private readonly PackageRepository PackageRepo;
        private readonly TailorSolutionForm Form;

        public ExportProblemSpaceCommand(ElementRepository elementRepo, PackageRepository packageRepo, TailorSolutionForm form)
        {
            ElementRepo = elementRepo;
            PackageRepo = packageRepo;
            Form = form;
        }

        public Unit Execute(EA.Package package)
        {
            var elements = from p in package.DescendantPackages()
                           from element in p.Elements()
                           select element.AsModelEntity();
            var diagrams = from p in package.DescendantPackages()
                           from diagram in p.Diagrams()
                           select diagram.AsModelEntity();

            var filters = Filter.Or("", new[]{
                Filter.And("Elements", e => e.Match<ModelEntity.Element>().IsDefined, new[] {
                    CreatePropertyFilter("Metatype", e => e.MetaType, elements),
                    CreatePropertyFilter("Type", e => e.Type, elements),
                    CreatePropertyFilter("Stereotype", e => e.Stereotype, elements),
                    CreateTaggedValueFilter(Common.IntellectualPropertyRights, elements),
                    CreateTaggedValueFilter(Common.OrganisationalReach, elements),
                    CreateTaggedValueFilter(Common.ProjectStage, elements),
                    CreateTaggedValueFilter(Common.Viewpoint, elements)
                }),
                Filter.And("Diagrams", e => e.Match<ModelEntity.Diagram>().IsDefined, new []{
                    CreatePropertyFilter("Meta Type", d => d.MetaType, diagrams),
                    CreatePropertyFilter("Type", d => d.Type, diagrams),
                    CreatePropertyFilter("Stereotype", d => d.Stereotype, diagrams)
                }),
                Filter.And("Packages", e => e.Match<ModelEntity.Package>().IsDefined, new IFilter<ModelEntity>[]{})
            });

            var hierarchy = CreatePackageHierarchy(package);

            Form.SelectFilter(filters, filter =>
            {
                return ApplyFilter(hierarchy, filter);
            }).Do(selectedFilter =>
            {
                throw new NotImplementedException();
            });

            return Unit.Instance;
        }

        private LabeledTree<ModelEntity, Unit> CreatePackageHierarchy(EA.Package root)
        {
            var subnodes = root.Elements().Select(e => LabeledTree.Node<ModelEntity, Unit>(e.AsModelEntity()))
                .Concat(root.Diagrams().Select(d => LabeledTree.Node<ModelEntity, Unit>(d.AsModelEntity())))
                .Concat(root.Packages().Select(p => CreatePackageHierarchy(p)));

            return LabeledTree.Node(root.AsModelEntity(),
                from subnode in subnodes
                select LabeledTree.Edge(Unit.Instance, subnode));
        }

        private LabeledTree<ModelEntity, Unit> ApplyFilter(LabeledTree<ModelEntity, Unit> tree, IFilter<ModelEntity> filter)
        {
            var edges = from edge in tree.Edges
                        where filter.Accept(edge.Target.Label)
                        select LabeledTree.Edge(Unit.Instance, ApplyFilter(edge.Target, filter));
            return LabeledTree.Node(tree.Label, edges);
        }

        private IFilter<ModelEntity> CreatePropertyFilter<T>(String name, Func<T, String> selectProperty, IEnumerable<T> allEntities) where T : ModelEntity
        {
            var values = allEntities.Aggregate(ImmutableHashSet.Create<Option<String>>(),
                (vs, entity) =>
                {
                    var value = selectProperty(entity);
                    if (value == "")
                        return vs.Add(Options.None<String>());
                    else
                        return vs.Add(Options.Some(value));
                });

            var filters = from value in values
                          orderby value.GetOrElse("")
                          select Filter.Create<ModelEntity>(
                            value.GetOrElse("<empty>"),
                            entity =>
                            {
                                return entity.Match<T, bool>(
                                    e => selectProperty(e).Equals(value.GetOrElse("")),
                                    () => false);
                            });

            return Filter.Or(name, filters);
        }

        private IFilter<ModelEntity> CreateTaggedValueFilter<T>(TaggedValue taggedValue, IEnumerable<T> allEntities)
            where T : ModelEntity
        {
            return CreatePropertyFilter(taggedValue.Name, entity => entity.Get(taggedValue).GetOrElse(""), allEntities);
        }

        private IFilter<ModelEntity> ModelFilter<T>(String name, Func<T, bool> accept) where T : ModelEntity
        {
            return Filter.Create<ModelEntity>(name, me => me.Match<T, bool>(e => accept(e), () => false));
        }

        public bool CanExecute(EA.Package _)
        {
            return true;
        }

        public ICommand<Option<ContextItem>, object> AsMenuCommand()
        {
            return this.Adapt((Option<ContextItem> contextItem) =>
            {
                return from ci in contextItem
                       from package in PackageRepo.GetPackage(ci.Guid)
                       select package;
            });
        }
    }
}
