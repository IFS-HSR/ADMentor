using ADMentor.ADTechnology;
using ADMentor.DataAccess;
using EAAddInBase;
using EAAddInBase.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace ADMentor.PopulateDependencies
{
    /// <summary>
    /// Proposes entities from the associated solution space that are not present in the current
    /// problem space. Selected entities will be instantiated and connected with the solution
    /// entity in question.
    /// </summary>
    sealed class PopulateDependenciesCommand : ICommand<SolutionEntity, EntityModified>
    {
        private readonly ModelEntityRepository Repo;

        private readonly IDependencySelector Selector;

        public PopulateDependenciesCommand(ModelEntityRepository repo, IDependencySelector selector)
        {
            Repo = repo;
            Selector = selector;
        }

        public EntityModified Execute(SolutionEntity solutionEntity)
        {
            Repo.SaveAllDiagrams();

            var modified =
                from currentDiagram in GetCurrentDiagramContaining(solutionEntity)
                from solution in SolutionInstantiationGraph.Create(Repo, solutionEntity)
                let solutionTree = solution.Graph.ToTree(DirectedLabeledGraph.TraverseEdgeOnlyOnce<ModelEntity.Connector>())
                from markedSolutionTree in Selector.GetSelectedDependencies(solutionTree)
                let markedSolution = solution.WithSelection(markedSolutionTree.NodeLabels)
                let targetPackage = Repo.FindPackageContaining(solutionEntity)
                let instantiatedSolution = markedSolution.InstantiateSelectedItems(targetPackage)
                let _1 = instantiatedSolution.InstantiateMissingSolutionConnectors()
                let _2 = instantiatedSolution.CreateDiagramElements(currentDiagram)
                select EntityModified.Modified;

            return modified.GetOrElse(EntityModified.NotModified);
        }

        public Boolean CanExecute(SolutionEntity element)
        {
            return GetCurrentDiagramContaining(element).IsDefined;
        }

        private Option<ModelEntity.Diagram> GetCurrentDiagramContaining(ModelEntity.Element element)
        {
            return from diagram in Repo.GetCurrentDiagram()
                   where diagram.GetObject(element).IsDefined
                   select diagram;
        }

        public ICommand<ModelEntity, EntityModified> AsEntityCreatedHandler()
        {
            return this.Adapt(
                (ModelEntity entity) =>
                    from solutionEntity in entity.TryCast<SolutionEntity>()
                    where solutionEntity.IsNew()
                    select solutionEntity);
        }
    }
}
