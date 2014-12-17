using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using EAAddInFramework;
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
    public class ExportProblemSpaceCommand : ICommand<ModelEntity.Package, Unit>
    {
        private readonly TailorPackageExportForm FilterForm;
        private readonly ModelEntityRepository Repo;
        private readonly XmlExporter.Factory ExporterFactory;
        private readonly SelectExportPathDialog ExportFileDialog;

        public ExportProblemSpaceCommand(ModelEntityRepository repo, TailorPackageExportForm form,
            XmlExporter.Factory exporterFactory, SelectExportPathDialog exportFileDialog)
        {
            Repo = repo;
            FilterForm = form;
            ExporterFactory = exporterFactory;
            ExportFileDialog = exportFileDialog;
        }

        public Unit Execute(ModelEntity.Package package)
        {
            var packages = package.SubPackages.Run();
            var elements = (from descendant in packages
                            from element in descendant.Elements
                            select element).Run();
            var diagrams = (from descendant in packages
                            from diagram in descendant.Diagrams
                            select diagram).Run();

            var taggedValueFilters = from tv in Technologies.AD.TaggedValues
                                     where tv.Type.TypeName.Equals("String") || tv.Type.TypeName.Equals("Enum")
                                     select CreateTaggedValueFilter(tv, elements);

            var filters = Filter.Or("", new[]{
                Filter.And("Elements", e => e.TryCast<ModelEntity.Element>().IsDefined, new[] {
                    CreatePropertyFilter("Metatype", elements, e => e.MetaType),
                    CreatePropertyFilter("Type", elements, e => e.Type),
                    CreatePropertyFilter("Stereotype", elements, e => e.Stereotype),
                    CreateKeywordFilter(elements)
                }.Concat(taggedValueFilters)),
                Filter.And("Diagrams", e => e.TryCast<ModelEntity.Diagram>().IsDefined, new []{
                    CreatePropertyFilter("Meta Type", diagrams, d => d.MetaType),
                    CreatePropertyFilter("Type", diagrams, d => d.Type),
                    CreatePropertyFilter("Stereotype", diagrams, d => d.Stereotype)
                }),
                Filter.And("Packages", e => e.TryCast<ModelEntity.Package>().IsDefined, new []{
                    CreateKeywordFilter(packages)
                })
            });

            var hierarchy = CreatePackageHierarchy(package);

            FilterForm.SelectFilter(filters, filter => ApplyFilter(hierarchy, filter))
                .Do(selectedHierarchy =>
                {
                    ExportFileDialog.WithSelectedFile(outStream =>
                    {
                        ExporterFactory.WithXmlExporter(package, exporter =>
                        {
                            exporter.Tailor(selectedHierarchy.NodeLabels);
                            exporter.WriteTo(outStream);
                        });
                    });
                });

            return Unit.Instance;
        }

        private LabeledTree<ModelEntity, Unit> CreatePackageHierarchy(ModelEntity.Package root)
        {
            var subnodes = root.Elements.Select(e => LabeledTree.Node<ModelEntity, Unit>(e))
                .Concat(root.Diagrams.Select(d => LabeledTree.Node<ModelEntity, Unit>(d)))
                .Concat(root.Packages.Select(p => CreatePackageHierarchy(p)));

            return LabeledTree.Node(root,
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

        private IFilter<ModelEntity> CreatePropertyFilter<T>(String name, IEnumerable<T> allEntities, Func<T, String> selectProperty) where T : ModelEntity
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
                          let filter = Filter.Create<T>(
                                value.GetOrElse("<empty>"),
                                entity => selectProperty(entity).Equals(value.GetOrElse("")))
                          select LiftFilter(filter);

            return Filter.Or(name, filters);
        }

        private IFilter<ModelEntity> CreateTaggedValueFilter<T>(ITaggedValue taggedValue, IEnumerable<T> allEntities)
            where T : ModelEntity
        {
            return CreatePropertyFilter(taggedValue.Name, allEntities, entity => entity.Get(taggedValue).GetOrElse(""));
        }

        private IFilter<ModelEntity> CreateKeywordFilter<T>(IEnumerable<T> allEntities)
            where T : ModelEntity
        {
            var keywords = allEntities.Aggregate(ImmutableHashSet.Create<String>() as IImmutableSet<String>, (ks, entity) =>
            {
                return ks.AddRange(entity.Keywords);
            });

            var filters = from keyword in keywords
                          orderby keyword
                          let filter = Filter.Create<T>(
                                keyword == "" ? "<empty>" : keyword,
                                entity => entity.Keywords.Contains(keyword))
                          select LiftFilter(filter);

            return Filter.Or("Keyword", filters);
        }

        /// <summary>
        /// Creates a filter over all model entities that accepts when the argument is of type T
        /// and <c>filter</c> accepts. If the filter argument is a model entity not in T the filter
        /// rejects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        private IFilter<ModelEntity> LiftFilter<T>(IFilter<T> filter) where T : ModelEntity
        {
            Func<ModelEntity, bool> accept =
                me => me.TryCast<T>().Fold(
                    e => filter.Accept(e),
                    () => false);
            return Filter.Create<ModelEntity>(filter.Name, accept);
        }

        public bool CanExecute(ModelEntity.Package _)
        {
            return true;
        }
    }
}
