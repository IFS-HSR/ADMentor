using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using EAAddInFramework;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.PopulateDependencies
{
    class PopulateDependenciesCommand : ICommand<EA.Element, EntityModified>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ElementRepository Repo;

        private readonly IDependencySelector Selector;

        private readonly DiagramRepository DiagramRepo;

        public PopulateDependenciesCommand(ElementRepository repo, DiagramRepository diagramRepo, IDependencySelector selector)
        {
            Repo = repo;
            DiagramRepo = diagramRepo;
            Selector = selector;
        }

        public EntityModified Execute(EA.Element element)
        {
            //var modified =
            //    from currentDiagram in GetCurrentDiagramContaining(element)
            //    from solution in SolutionInstantiationTree.Create(Repo, element)
            //    from selectedSolution in Selector.GetSelectedDependencies(solution)
            //    let targetPackage = Repo.FindPackageContaining(element)
            //    let instantiatedSolution = SolutionInstantiationTree.InstantiateSelectedItems(Repo, targetPackage, selectedSolution)
            //    let _ = SolutionInstantiationTree.CreateDiagramElements(Repo, DiagramRepo, currentDiagram, instantiatedSolution)
            //    select EntityModified.Modified;
            var modified =
                from currentDiagram in GetCurrentDiagramContaining(element)
                from solution in SolutionInstantiationGraph.Create(Repo, element)
                let solutionTree = solution.ToTree(DirectedLabeledGraph.TraverseEdgeOnlyOnce(new DependencyGraph.ConnectorComparer()))
                from markedSolutionTree in Selector.GetSelectedDependencies(solutionTree)
                let markedSolution = SolutionInstantiationGraph.CopySelection(solution, markedSolutionTree)
                let targetPackage = Repo.FindPackageContaining(element)
                let instantiatedSolution = SolutionInstantiationGraph.InstantiateSelectedItems(Repo, targetPackage, markedSolution)
                let _1 = SolutionInstantiationGraph.InstantiateMissingSolutionConnectors(Repo, instantiatedSolution)
                let _2 = SolutionInstantiationGraph.CreateDiagramElements(DiagramRepo, currentDiagram, instantiatedSolution)
                select EntityModified.Modified;

            return modified.GetOrElse(EntityModified.NotModified);
        }

        public Boolean CanExecute(EA.Element element)
        {
            return GetCurrentDiagramContaining(element).IsDefined;
        }

        private Option<EA.Diagram> GetCurrentDiagramContaining(EA.Element element)
        {
            return from diagram in DiagramRepo.GetCurrentDiagram()
                   where DiagramRepo.Contains(diagram, element)
                   select diagram;
        }

        public ICommand<Option<ContextItem>, object> AsMenuCommand()
        {
            return this.Adapt<Option<ContextItem>, EA.Element, object>(
                contextItem => from ci in contextItem from e in Repo.GetElement(ci.Guid) select e);
        }

        public ICommand<Func<EA.Element>, EntityModified> AsElementCreatedHandler()
        {
            return this.Adapt<Func<EA.Element>, EA.Element, EntityModified>(
                getElement =>
                {
                    var element = getElement();
                    if (element.IsNew())
                    {
                        return Options.Some(element);
                    }
                    else
                    {
                        return Options.None<EA.Element>();
                    }
                });
        }
    }
}
