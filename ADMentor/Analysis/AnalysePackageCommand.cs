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
using System.Windows.Forms;
using EAAddInBase.Utils;

namespace ADMentor.Analysis
{
    /// <summary>
    /// Collects various ADMentor related statistics in a package.
    /// </summary>
    sealed class AnalysePackageCommand : ICommand<ModelEntity.Package, Unit>
    {
        private readonly ModelEntityRepository Repository;
        private readonly DisplayMetricsForm DisplayMetricsForm;

        public AnalysePackageCommand(ModelEntityRepository repo, DisplayMetricsForm displayMetricsForm)
        {
            Repository = repo;
            DisplayMetricsForm = displayMetricsForm;
        }

        public Unit Execute(ModelEntity.Package package)
        {
            var packages = package.SubPackages.Run();

            var elements = (from p in packages
                            from e in p.Elements
                            select e).Run();

            var elementsPerPackage = (from p in packages
                                      select Tuple.Create(p, p.Elements.Run())).Run();

            var optionsPerProblem = FindTargetsPerSource<Problem, OptionEntity>(elements, ConnectorStereotypes.AddressedBy);
            var problemsPerOption = FindTargetsPerSource<OptionEntity, Problem>(elements, ConnectorStereotypes.AddressedBy);

            var optionOccsPerProblemOcc = FindTargetsPerSource<ProblemOccurrence, OptionOccurrence>(elements, ConnectorStereotypes.AddressedBy);
            var problemOccsPerOptionOcc = FindTargetsPerSource<OptionOccurrence, ProblemOccurrence>(elements, ConnectorStereotypes.AddressedBy);

            var oosPerState = from e in elements
                              from oo in e.TryCast<OptionOccurrence>()
                              group oo by oo.State into g
                              select Tuple.Create(g.Key.Name, g.Count());

            var posPerState = from e in elements
                              from po in e.TryCast<ProblemOccurrence>()
                              group po by po.State into g
                              select Tuple.Create(g.Key.Name, g.Count());

            var valuesPerTag = from tv in Technologies.AD.TaggedValues
                               where !tv.Type.TypeName.Equals("DateTime")
                               from v in ValueOccurrences(tv, elements)
                               select Tuple.Create(tv.Name, v.Item1, v.Item2);

            var entries =
                (from e in new[] { 
                    new MetricEntry("Element", elements.Count()), 
                    new MetricEntry("Package", packages.Count())
                 }
                 select e.Copy(category: "Metadata", @group: "Type")).Concat(
                 from e in new[] { 
                     new MetricEntry("Problem", elements.Count(e => e is Problem)),
                    new MetricEntry("Option", elements.Count(e => e is OptionEntity)),
                    new MetricEntry("Problem Occurrence", elements.Count(e => e is ProblemOccurrence)),
                    new MetricEntry("Option Occurrence", elements.Count(e => e is OptionOccurrence))
                 }
                 select e.Copy(category: "Metadata", @group: "Element Type")).Concat(
                 from e in posPerState
                 select new MetricEntry("Tagged Values", "Problem State", e.Item1, e.Item2)).Concat(
                 from e in oosPerState
                 select new MetricEntry("Tagged Values", "Option State", e.Item1, e.Item2)).Concat(
                 from e in valuesPerTag
                 select new MetricEntry("Tagged Values", e.Item1, e.Item2, e.Item3)).Concat(
                 from e in CreateSummary(elementsPerPackage)
                 select new MetricEntry("Statistics", "Elements per Package", e.Item1, e.Item2)).Concat(
                 from e in CreateSummary(optionsPerProblem)
                 select new MetricEntry("Statistics", "Options per Problem", e.Item1, e.Item2)).Concat(
                 from e in CreateSummary(problemsPerOption)
                 select new MetricEntry("Statistics", "Problems per Options", e.Item1, e.Item2)).Concat(
                 from e in CreateSummary(optionOccsPerProblemOcc)
                 select new MetricEntry("Statistics", "Option Occ. per Problem Occ.", e.Item1, e.Item2)).Concat(
                 from e in CreateSummary(problemOccsPerOptionOcc)
                 select new MetricEntry("Statistics", "Problem Occ. per Option Occ.", e.Item1, e.Item2));

            DisplayMetricsForm.Display(entries);

            return Unit.Instance;
        }

        private IEnumerable<Tuple<String, int>> ValueOccurrences(TaggedValue tv, IEnumerable<ModelEntity.Element> elements)
        {
            return from e in elements
                   from val in e.Get(tv)
                   let valOrEmpty = val.Equals("") ? "<empty>" : val
                   group e by valOrEmpty into g
                   orderby g.Key
                   select Tuple.Create(g.Key, g.Count());
        }

        private IEnumerable<Tuple<TSource, IEnumerable<TTarget>>> FindTargetsPerSource<TSource, TTarget>(
            IEnumerable<ModelEntity.Element> elements, ConnectorStereotype connectorStype)
            where TSource : ModelEntity.Element
            where TTarget : ModelEntity.Element
        {
            return (from e in elements
                    from source in e.TryCast<TSource>()
                    let targets = source.ElementsConnectedBy<TTarget>(connectorStype, Repository.GetElement).Run()
                    select Tuple.Create(source, targets)).Run();
        }

        private IEnumerable<Tuple<String, String>> CreateSummary<TKey, TElement>(IEnumerable<Tuple<TKey, IEnumerable<TElement>>> groups)
        {
            Func<Tuple<TKey, IEnumerable<TElement>>, int> selector = (kv) => kv.Item2.Count();
            var data = groups.IsEmpty() ?
                Tuple.Create("-", "-", "-") :
                Tuple.Create(groups.Min(selector).ToString(), groups.Average(selector).ToString("N"), groups.Max(selector).ToString());
            return new[]{
                Tuple.Create("Min", data.Item1),
                Tuple.Create("Avg", data.Item2),
                Tuple.Create("Max", data.Item3)
            };
        }

        public bool CanExecute(ModelEntity.Package _)
        {
            return true;
        }
    }
}
