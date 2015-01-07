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
            var filterTags = from tv in Technologies.AD.TaggedValues
                             where tv.Type.TypeName.Equals("String") || tv.Type.TypeName.Equals("Enum")
                             select tv;

            var propertyTree = PropertyTree.Create(package, entity =>
            {
                var props = new Dictionary<String, IEnumerable<String>>{
                    {"Type", new[]{entity.Type}},
                    {"Metatype", new[]{entity.MetaType}}
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
                    return Filter.Or(prop.Key, prop.Select(value =>
                    {
                        return Filter.Create<PropertyTree>(value, pt => pt.Properties.Contains(Tuple.Create(prop.Key, value)));
                    }));
                }));

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
        public static IImmutableSet<String> CollectTaggedValues(ModelEntity entity, ITaggedValue tv, Func<int, Option<ModelEntity.Element>> getElementById)
        {
            return entity.Match<ModelEntity, IImmutableSet<String>>()
                .Case<OptionEntity>(o =>
                    o.Get(tv)
                    .Select(v => ImmutableHashSet.Create(v))
                    .GetOrElse(() => o.GetProblems(getElementById)
                        .SelectMany(p => CollectTaggedValues(p, tv, getElementById))
                        .ToImmutableHashSet()))
                .Case<OptionOccurrence>(o =>
                    o.Get(tv)
                    .Select(v => ImmutableHashSet.Create(v))
                    .GetOrElse(() => o.GetAssociatedProblemOccurrences(getElementById)
                        .SelectMany(p => CollectTaggedValues(p, tv, getElementById))
                        .ToImmutableHashSet()))
                .Case<ModelEntity.Element>(e => e.Get(tv).ToImmutableHashSet())
                .Default(_ => ImmutableHashSet.Create<String>());
        }

        public bool CanExecute(ModelEntity.Package _)
        {
            return true;
        }
    }
}
