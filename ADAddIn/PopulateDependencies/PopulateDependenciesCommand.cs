using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.PopulateDependencies
{
    class PopulateDependenciesCommand : ICommand<SolutionEntity, EntityModified>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ModelEntityRepository Repo;

        private readonly IDependencySelector Selector;

        private readonly DiagramRepository DiagramRepo;

        public PopulateDependenciesCommand(ModelEntityRepository repo, DiagramRepository diagramRepo, IDependencySelector selector)
        {
            Repo = repo;
            DiagramRepo = diagramRepo;
            Selector = selector;
        }

        public EntityModified Execute(SolutionEntity solutionEntity)
        {
            var modified =
                from currentDiagram in GetCurrentDiagramContaining(solutionEntity)
                from solution in SolutionInstantiationGraph.Create(Repo, solutionEntity)
                let solutionTree = solution.Graph.ToTree(DirectedLabeledGraph.TraverseEdgeOnlyOnce<ModelEntity.Connector>())
                from markedSolutionTree in Selector.GetSelectedDependencies(solutionTree)
                let markedSolution = solution.WithSelection(markedSolutionTree.NodeLabels)
                let targetPackage = Repo.FindPackageContaining(solutionEntity)
                let instantiatedSolution = markedSolution.InstantiateSelectedItems(targetPackage)
                let _1 = instantiatedSolution.InstantiateMissingSolutionConnectors()
                let _2 = instantiatedSolution.CreateDiagramElements(DiagramRepo, currentDiagram)
                select EntityModified.Modified;

            return modified.GetOrElse(EntityModified.NotModified);
        }

        public Boolean CanExecute(SolutionEntity element)
        {
            return GetCurrentDiagramContaining(element).IsDefined;
        }

        private Option<EA.Diagram> GetCurrentDiagramContaining(ModelEntity.Element element)
        {
            return from diagram in DiagramRepo.GetCurrentDiagram()
                   where DiagramRepo.Contains(diagram, element.EaObject)
                   select diagram;
        }

        public ICommand<Option<ModelEntity>, object> AsMenuCommand()
        {
            return this.Adapt(
                (Option<ModelEntity> contextItem) =>
                    from ci in contextItem
                    from solutionEntity in ci.Match<SolutionEntity>()
                    select solutionEntity);
        }

        public ICommand<ModelEntity, EntityModified> AsEntityCreatedHandler()
        {
            return this.Adapt(
                (ModelEntity entity) =>
                    from solutionEntity in entity.Match<SolutionEntity>()
                    where solutionEntity.IsNew()
                    select solutionEntity);
        }
    }
}
