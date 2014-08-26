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
                   where classifier.Is(ElementStereotypes.Problem) || classifier.Is(ElementStereotypes.Option)
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
            problemSpace.TraverseEdgesBF((source, edge, target) =>
            {
                source.Instance.Do(solutionSource =>
                {
                    target.Instance.Do(solutionTarget =>
                    {
                        repo.GetStereotype(edge).Do(stype =>
                        {
                            var connectsAlternativeToProblem = 
                                stype == ConnectorStereotypes.HasAlternative && solutionSource.Is(ElementStereotypes.OptionOccurrence);
                            var alreadyExisting = solutionSource.Connectors.Cast<EA.Connector>().Any(c =>
                            {
                                return c.Is(stype) && (c.SupplierID == solutionTarget.ElementID || c.ClientID == solutionTarget.ElementID);
                            });

                            if (!connectsAlternativeToProblem && !alreadyExisting)
                            {
                                stype.Create(solutionSource, solutionTarget);
                            }
                        });
                    });
                });
            });
            return Unit.Instance;
        }

        public static DirectedLabeledGraph<SolutionInstantiation, EA.Connector> CopySelection(
            DirectedLabeledGraph<SolutionInstantiation, EA.Connector> solution,
            LabeledTree<SolutionInstantiation, EA.Connector> markedSolutionTree)
        {
            return solution.MapNodeLabels(si =>
            {
                var selected = markedSolutionTree.NodeLabels.Any(n => n.Element.ElementGUID == si.Element.ElementGUID && n.Selected);
                return si.Copy(selected: selected);
            });
        }

        internal static Unit CreateDiagramElements(
            DiagramRepository diagramRepo, EA.Diagram diagram,
            DirectedLabeledGraph<SolutionInstantiation, EA.Connector> problemSpace)
        {
            var siblings = new Dictionary<SolutionInstantiation, int>();

            problemSpace.TraverseEdgesBF((from, via, to) =>
            {
                var leftHandSiblings = siblings.ContainsKey(from) ? siblings[from] : 0;

                to.Instance.Do(toInstance =>
                {
                    if (!diagramRepo.FindDiagramObject(diagram, toInstance).IsDefined)
                    {
                        from.Instance.Do(fromInstance =>
                        {
                            var parentObject = diagramRepo.FindDiagramObject(diagram, fromInstance).Value;

                            var verticalOffset = leftHandSiblings * 110 - 40;
                            var horizontalOffset = -200 - leftHandSiblings * 20;

                            diagramRepo.AddToDiagram(diagram, toInstance,
                                parentObject.left + verticalOffset, parentObject.top + horizontalOffset,
                                parentObject.right - parentObject.left, parentObject.bottom - parentObject.top);

                            siblings[from] = leftHandSiblings + 1;
                        });
                    }
                });
            });
            diagramRepo.ReloadDiagram(diagram);

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
