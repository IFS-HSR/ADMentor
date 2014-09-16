﻿using AdAddIn.ADTechnology;
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

        public ExportProblemSpaceCommand(ModelEntityRepository repo, TailorPackageExportForm form, XmlExporter.Factory exporterFactory)
        {
            Repo = repo;
            FilterForm = form;
            ExporterFactory = exporterFactory;
        }

        public Unit Execute(ModelEntity.Package package)
        {
            var packages = package.AllDescendants().Run();
            var elements = from descendant in packages
                           from element in descendant.Elements()
                           select element;
            var diagrams = from descendant in packages
                           from diagram in descendant.Diagrams()
                           select diagram;

            var filters = Filter.Or("", new[]{
                Filter.And("Elements", e => e.Match<ModelEntity.Element>().IsDefined, new[] {
                    CreatePropertyFilter("Metatype", elements, e => e.MetaType),
                    CreatePropertyFilter("Type", elements, e => e.Type),
                    CreatePropertyFilter("Stereotype", elements, e => e.Stereotype),
                    CreateKeywordFilter(elements),
                    CreateTaggedValueFilter(Common.IntellectualPropertyRights, elements),
                    CreateTaggedValueFilter(Common.OrganisationalReach, elements),
                    CreateTaggedValueFilter(Common.ProjectStage, elements),
                    CreateTaggedValueFilter(Common.Viewpoint, elements),
                    CreateTaggedValueRefFilter(StakeholderRoles.StakeholderRoleRef, elements)
                }),
                Filter.And("Diagrams", e => e.Match<ModelEntity.Diagram>().IsDefined, new []{
                    CreatePropertyFilter("Meta Type", diagrams, d => d.MetaType),
                    CreatePropertyFilter("Type", diagrams, d => d.Type),
                    CreatePropertyFilter("Stereotype", diagrams, d => d.Stereotype)
                }),
                Filter.And("Packages", e => e.Match<ModelEntity.Package>().IsDefined, new []{
                    CreateKeywordFilter(packages)
                })
            });

            var hierarchy = CreatePackageHierarchy(package);

            FilterForm.SelectFilter(filters, filter =>
            {
                return ApplyFilter(hierarchy, filter);
            }).Do(selectedHierarchy =>
            {
                new SelectExportPathDialog().WithSelectedFile(outStream =>
                {
                    var selectedEntities = selectedHierarchy
                        .NodeLabels
                        .Run();

                    ExporterFactory.WithXmlExporter(package, exporter =>
                    {
                        exporter.RemoveEntities(guid => !selectedEntities.Select(e => e.Guid).Contains(guid)
                            && !IsExportableConnector(guid, selectedEntities));
                        exporter.WriteTo(outStream);
                    });
                });
            });

            return Unit.Instance;
        }

        private bool IsExportableConnector(String guid, IEnumerable<ModelEntity> selectedEntities)
        {
            return Repo.GetConnector(guid).Match(
                connector =>
                {
                    return (from source in connector.Source(Repo.GetElement)
                            from target in connector.Target(Repo.GetElement)
                            select selectedEntities.Any(e => source.Equals(e)) && selectedEntities.Any(e => target.Equals(e)))
                           .GetOrElse(false);
                },
                () => false);
        }

        private LabeledTree<ModelEntity, Unit> CreatePackageHierarchy(ModelEntity.Package root)
        {
            var subnodes = root.Elements().Select(e => LabeledTree.Node<ModelEntity, Unit>(e))
                .Concat(root.Diagrams().Select(d => LabeledTree.Node<ModelEntity, Unit>(d)))
                .Concat(root.Packages().Select(p => CreatePackageHierarchy(p)));

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
            return CreatePropertyFilter(taggedValue.Name, allEntities, entity => entity.Get(taggedValue).GetOrElse(""));
        }

        private IFilter<ModelEntity> CreateTaggedValueRefFilter<T>(TaggedValue taggedValue, IEnumerable<T> allEntities)
            where T : ModelEntity
        {
            return CreatePropertyFilter(taggedValue.Name, allEntities, entity =>
            {
                return (from value in entity.Get(taggedValue)
                        from referencedElement in Repo.GetElement(value)
                        select referencedElement.Name).GetOrElse("");
            });
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
                          select ModelFilter<T>(
                                keyword == "" ? "<empty>" : keyword,
                                entity => entity.Keywords.Contains(keyword));

            return Filter.Or("Keyword", filters);
        }

        private IFilter<ModelEntity> ModelFilter<T>(String name, Func<T, bool> accept) where T : ModelEntity
        {
            return Filter.Create<ModelEntity>(name, me => me.Match<T, bool>(e => accept(e), () => false));
        }

        public bool CanExecute(ModelEntity.Package _)
        {
            return true;
        }

        public ICommand<Option<ModelEntity>, object> AsMenuCommand()
        {
            return this.Adapt((Option<ModelEntity> contextItem) =>
            {
                return from ci in contextItem
                       from package in ci.Match<ModelEntity.Package>()
                       select package;
            });
        }
    }
}
