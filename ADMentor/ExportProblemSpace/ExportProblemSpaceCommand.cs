using ADMentor.ADTechnology;
using ADMentor.DataAccess;
using EAAddInBase;
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
    /// <summary>
    /// Exports a tailored Problem Space to an XMI file.
    /// </summary>
    sealed class ExportProblemSpaceCommand : ICommand<ModelEntity.Package, Unit>
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
            var filterTags = from tv in Technologies.AD.TaggedValues
                             where tv.Type.TypeName.Equals("String") || tv.Type.TypeName.Equals("Enum")
                             select tv;

            var propertyTree = PropertyTree.Create(package, entity =>
            {
                var props = new Dictionary<String, IEnumerable<Option<String>>>{
                    {"Type", new[]{entity.Type.AsOption()}},
                    {"Metatype", new[]{entity.MetaType.AsOption()}},
                    {"Stereotype", new[]{entity.Stereotype.AsOption()}},
                    {"Keyword", entity.Keywords.Select(kw => kw.AsOption())},
                    {"Author", new[]{entity.Author}},
                    {"Version", new[]{entity.Version.AsOption()}},
                    {"Status", new[]{entity.Status}}
                };

                filterTags.ForEach(tv =>
                {
                    props.Add(tv.Name, CollectTaggedValues(entity, tv, Repo.GetElement));
                });

                return props;
            });

            var filters = Filter.And("",
                propertyTree.Properties.ToLookup(p => p.Item1, p => p.Item2).Select(prop =>
                {
                    var alternatives = prop.Select(value =>
                    {
                        var filterName = value.Select(v => v == "" ? "<empty>" : v).GetOrElse("<none>");
                        return Filter.Create<PropertyTree>(filterName, pt => pt.Properties.Contains(Tuple.Create(prop.Key, value)));
                    }).OrderBy(alt => alt.Name);

                    return Filter.Or(prop.Key, alternatives);
                }).OrderBy(propFilter => propFilter.Name));

            FilterForm.SelectFilter(filters, propertyTree.ApplyFilter)
                .Do(selectedHierarchy =>
                {
                    ExportFileDialog.WithSelectedFile(outStream =>
                    {
                        ExporterFactory.WithXmlExporter(package, exporter =>
                        {
                            exporter.Tailor(selectedHierarchy.AllEntities());
                            exporter.WriteTo(outStream);
                        });
                    });
                });

            return Unit.Instance;
        }

        private static IImmutableSet<Option<String>> CollectTaggedValues(ModelEntity entity, TaggedValue tv, Func<int, Option<ModelEntity.Element>> getElementById)
        {
            return entity.Match<ModelEntity, IImmutableSet<Option<String>>>()
                .Case<OptionEntity>(o =>
                    o.Get(tv)
                    .Select(v => ImmutableHashSet.Create(Options.Some(v)))
                    .GetOrElse(() => o.Problems(getElementById)
                        .SelectMany(p => CollectTaggedValues(p, tv, getElementById))
                        .ToImmutableHashSet()))
                .Case<OptionOccurrence>(o =>
                    o.Get(tv)
                    .Select(v => ImmutableHashSet.Create(Options.Some(v)))
                    .GetOrElse(() => o.AssociatedProblemOccurrences(getElementById)
                        .SelectMany(p => CollectTaggedValues(p, tv, getElementById))
                        .ToImmutableHashSet()))
                .Case<ModelEntity.Element>(e => ImmutableHashSet.Create(e.Get(tv)))
                .Default(_ => ImmutableHashSet.Create<Option<String>>());
        }

        public bool CanExecute(ModelEntity.Package _)
        {
            return true;
        }
    }
}
