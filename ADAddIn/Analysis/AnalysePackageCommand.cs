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
using System.Windows.Forms;
using Utils;

namespace AdAddIn.Analysis
{
    public class AnalysePackageCommand : ICommand<ModelEntity.Package, Unit>
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

            var elementsPerPackage = from p in packages
                                     select Tuple.Create(p, p.Elements.Run());

            var optionsPerProblem = FindTargetsPerSource<Problem, OptionEntity>(elements, ConnectorStereotypes.AddressedBy);
            var problemsPerOption = FindTargetsPerSource<OptionEntity, Problem>(elements, ConnectorStereotypes.AddressedBy);

            var optionOccsPerProblemOcc = FindTargetsPerSource<ProblemOccurrence, OptionOccurrence>(elements, ConnectorStereotypes.AddressedBy);
            var problemOccsPerOptionOcc = FindTargetsPerSource<OptionOccurrence, ProblemOccurrence>(elements, ConnectorStereotypes.AddressedBy);

            var oosPerState = from e in elements
                              from oo in e.TryCast<OptionOccurrence>()
                              group oo by oo.State into g
                              select Metrics.Entry(g.Key.Name, g.Count());

            var posPerState = from e in elements
                              from po in e.TryCast<ProblemOccurrence>()
                              group po by po.State into g
                              select Metrics.Entry(g.Key.Name, g.Count());

            var metrics = Metrics.Category(package.Name,
                Metrics.Category("Common",
                    Metrics.Entry("Elements", elements.Count()),
                    Metrics.Entry("Packages", packages.Count()),
                    Metrics.Entry("Elements per Package", CreateSummary(elementsPerPackage))),
                Metrics.Category("Problem Space",
                    Metrics.Entry("Problems", elements.Count(e => e is Problem)),
                    Metrics.Entry("Options", elements.Count(e => e is OptionEntity)),
                    Metrics.Entry("Options per Problem", CreateSummary(optionsPerProblem)),
                    Metrics.Entry("Problems per Option", CreateSummary(problemsPerOption))),
                Metrics.Category("Solution Space",
                    Metrics.Entry("Problem Occurrences", elements.Count(e => e is ProblemOccurrence)),
                    Metrics.Entry("Option Occurrences", elements.Count(e => e is OptionOccurrence)),
                    Metrics.Entry("Options per Problem", CreateSummary(optionOccsPerProblemOcc)),
                    Metrics.Entry("Problems per Option", CreateSummary(problemOccsPerOptionOcc)),
                    Metrics.Category("Problem States", posPerState.ToArray()),
                    Metrics.Category("Option States", oosPerState.ToArray())));

            DisplayMetricsForm.Display(metrics);

            return Unit.Instance;
        }

        private IEnumerable<Tuple<TSource, IEnumerable<TTarget>>> FindTargetsPerSource<TSource, TTarget>(
            IEnumerable<ModelEntity.Element> elements, ConnectorStereotype connectorStype)
            where TSource : ModelEntity.Element
            where TTarget : ModelEntity.Element
        {
            return (from e in elements
                    from source in e.TryCast<TSource>()
                    let targets = (from c in source.Connectors
                                   where c.Is(connectorStype)
                                   from otherEnd in c.OppositeEnd(source, Repository.GetElement)
                                   from target in otherEnd.TryCast<TTarget>()
                                   select target).Run()
                    select Tuple.Create(source, targets)).Run();
        }

        private String CreateSummary<TKey, TElement>(IEnumerable<Tuple<TKey, IEnumerable<TElement>>> groups)
        {
            Func<Tuple<TKey, IEnumerable<TElement>>, int> selector = (kv) => kv.Item2.Count();
            var data = groups.IsEmpty() ?
                Tuple.Create("-", "-", "-") :
                Tuple.Create(groups.Min(selector).ToString(), groups.Average(selector).ToString("N"), groups.Max(selector).ToString());
            return String.Format("Min {0} / Avg {1} / Max {2}", data.Item1, data.Item2, data.Item3);
        }

        public bool CanExecute(ModelEntity.Package _)
        {
            return true;
        }
    }
}
