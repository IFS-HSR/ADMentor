using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
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

        public AnalysePackageCommand(ModelEntityRepository repo)
        {
            Repository = repo;
        }

        public Unit Execute(ModelEntity.Package package)
        {
            var packages = package.SubPackages.Run();

            var elements = (from p in packages
                            from e in p.Elements
                            select e).Run();

            var elementsPerPackage = from p in packages
                                     from e in p.Elements
                                     group e by p;

            var optionsPerProblem = from e in elements
                                    from o in e.Match<OptionEntity>()
                                    from c in e.Connectors
                                    where c.Is(ConnectorStereotypes.AddressedBy)
                                    from target in c.OppositeEnd(e, Repository.GetElement)
                                    from p in target.Match<Problem>()
                                    group o by p;

            var optionOccurrences = (from e in elements
                                     from oo in e.Match<OptionOccurrence>()
                                     select oo).Run();

            var oosPerPo = from oo in optionOccurrences
                           from c in oo.Connectors
                           where c.Is(ConnectorStereotypes.AddressedBy)
                           from target in c.OppositeEnd(oo, Repository.GetElement)
                           from po in target.Match<ProblemOccurrence>()
                           group oo by po;

            var oosPerState = from oo in optionOccurrences
                              group oo by oo.State into g
                              select Entry(g.Key.Name, g.Count());

            var metrics = Category(package.Name,
                Category("Common",
                    Entry("Elements", elements.Count()),
                    Entry("Packages", packages.Count()),
                    Entry("Elements per Package", CreateSummary(elementsPerPackage, g => g.Count()))),
                Category("Problem Space",
                    Entry("Problems", elements.Count(e => e is Problem)),
                    Entry("Options", elements.Count(e => e is OptionEntity)),
                    Entry("Options per Problem", CreateSummary(optionsPerProblem, g => g.Count()))),
                Category("Solution Space",
                    Entry("Problem Occurrences", elements.Count(e => e is ProblemOccurrence)),
                    Entry("Option Occurrences", optionOccurrences.Count()),
                    Entry("Options per Problem", CreateSummary(oosPerPo, g => g.Count())),
                    Category("States", oosPerState.ToArray())));

            MessageBox.Show(metrics.ToString());

            return Unit.Instance;
        }

        private String CreateSummary<TKey, TElement>(IEnumerable<IGrouping<TKey, TElement>> groups, Func<IGrouping<TKey, TElement>, int> selector)
        {
            var data = groups.IsEmpty() ?
                Tuple.Create("-", "-", "-") :
                Tuple.Create(groups.Min(selector).ToString(), groups.Average(selector).ToString("N"), groups.Max(selector).ToString());
            return String.Format("Min {0} / Avg {1} / Max {2}", data.Item1, data.Item2, data.Item3);
        }

        public bool CanExecute(ModelEntity.Package _)
        {
            return true;
        }

        private Metric Category(String name, params Metric[] members)
        {
            return new Category(name, members);
        }

        private Metric Entry<T>(String key, T value)
        {
            return new Entry<T>(key, value);
        }
    }

    interface Metric
    {
        String ToString(String prefix);
    }

    class Category : Metric
    {
        public Category(String name, IEnumerable<Metric> members)
        {
            Name = name;
            Members = members;
        }

        public string Name { get; private set; }

        public IEnumerable<Metric> Members { get; private set; }

        public override string ToString()
        {
            return ToString("");
        }

        public string ToString(String prefix)
        {
            return String.Format("{0}{1}:\n{2}", prefix, Name, Members.Select(m => m.ToString(prefix + "  ")).Join("\n"));
        }
    }

    class Entry<T> : Metric
    {
        public Entry(String key, T value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; private set; }

        public T Value { get; private set; }

        public override string ToString()
        {
            return ToString("");
        }

        public string ToString(String prefix)
        {
            return String.Format("{0}{1}: {2}", prefix, Key, Value.ToString());
        }
    }
}
