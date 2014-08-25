using AdAddIn.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using EAAddInFramework;
using AdAddIn.ADTechnology;

namespace AdAddIn.PopulateDependencies
{
    public static class SolutionInstantiationGraph
    {
        public static DirectedLabeledGraph<SolutionInstantiation, EA.Connector> Create(
            params Tuple<SolutionInstantiation, EA.Connector, SolutionInstantiation>[] edges)
        {
            return DirectedLabeledGraph.Create(
                EqualityComparer<SolutionInstantiation>.Default,
                new DependencyGraph.ConnectorComparer(),
                edges);
        }

        public static Option<DirectedLabeledGraph<SolutionInstantiation, EA.Connector>> Create(ElementRepository repo, EA.Element solutionItem)
        {
            return from classifier in repo.GetElement(solutionItem.ClassifierID)
                   where classifier.Is(ElementStereotypes.Problem)
                   let problemSpace = DependencyGraph.Create(repo, classifier, DependencyGraph.TraverseOnlyADConnectors)
                   let solution = DependencyGraph.Create(repo, solutionItem, DependencyGraph.TraverseOnlyADConnectors)
                   select Compare(problemSpace, solution);
        }

        private static DirectedLabeledGraph<SolutionInstantiation, EA.Connector> Compare(
            DirectedLabeledGraph<EA.Element, EA.Connector> problemSpace,
            DirectedLabeledGraph<EA.Element, EA.Connector> solution)
        {
            return problemSpace.MapNodeLabels<SolutionInstantiation>(problemItem =>
            {
                var instance = solution.NodeLabels.FirstOption(solutionItem => solutionItem.ClassifierID == problemItem.ElementID);
                return new SolutionInstantiation(problemItem, instance);
            });
        }

        public static DirectedLabeledGraph<SolutionInstantiation, EA.Connector> InstantiateSelectedItems(
            ElementRepository repo, EA.Package package,
            DirectedLabeledGraph<SolutionInstantiation, EA.Connector> problemSpace)
        {
            return problemSpace.MapNodeLabels(problemItem =>
            {
                if (problemItem.Selected && !problemItem.Instance.IsDefined)
                {
                    var instance = repo.Instanciate(problemItem.Element, package);
                    return problemItem.Copy(instance: instance);
                }
                else
                {
                    return problemItem;
                }
            });
        }

        public static Unit InstantiateMissingSolutionConnectors(
            ElementRepository repo,
            DirectedLabeledGraph<SolutionInstantiation, EA.Connector> problemSpace)
        {
            problemSpace.TraverseEdgesBF(problemSpace.NodeLabels.First(), (source, edge, target) =>
            {
                source.Instance.Do(solutionSource =>
                {
                    target.Instance.Do(solutionTarget =>
                    {
                        repo.GetStereotype(edge).Do(stype =>
                        {
                            stype.Create(solutionSource, solutionTarget);
                        });
                    });
                });
            });
            return Unit.Instance;
        }
    }

    public class SolutionInstantiation : IEquatable<SolutionInstantiation>
    {
        public SolutionInstantiation(EA.Element element, EA.Element instance, bool selected = false) : this(element, instance.AsOption(), selected) { }

        public SolutionInstantiation(EA.Element element, Option<EA.Element> instance = null, bool selected = false)
        {
            Element = element;
            Instance = instance ?? Options.None<EA.Element>();
            Selected = selected;
        }

        public EA.Element Element { get; private set; }

        public Option<EA.Element> Instance { get; private set; }

        public bool Selected { get; private set; }

        public bool Equals(SolutionInstantiation other)
        {
            return Element.ElementGUID == other.Element.ElementGUID
                && Instance.IsDefined == other.Instance.IsDefined
                && Instance.Match(e => e.ElementGUID == other.Instance.Value.ElementGUID, () => true)
                && Selected == other.Selected;
        }

        public SolutionInstantiation Copy(EA.Element element = null, Option<EA.Element> instance = null, bool? selected = null)
        {
            return new SolutionInstantiation(element ?? Element, instance ?? Instance, selected ?? Selected);
        }
    }
}
