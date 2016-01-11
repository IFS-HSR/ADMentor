using ADMentor.ADTechnology;
using ADMentor.DataAccess;
using ADMentor.PopulateDependencies;
using EAAddInBase;
using EAAddInBase.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace ADMentor.InstantiateProblemSpace
{
    /// <summary>
    /// Creates a Problem Space from a Solution Space.
    /// </summary>
    sealed class InstantiateProblemSpaceCommand : ICommand<ModelEntity.Package, Unit>
    {
        private readonly InstantiateSolutionForm SolutionNameForm;
        private readonly ModelEntityRepository Repo;

        public InstantiateProblemSpaceCommand(ModelEntityRepository repo, InstantiateSolutionForm solutionNameForm)
        {
            Repo = repo;
            SolutionNameForm = solutionNameForm;
        }

        public Unit Execute(ModelEntity.Package problemSpace)
        {
            SolutionNameForm.GetSolutionName().Do(solutionName =>
            {
                Repo.SaveAllDiagrams();

                var parentPackage = problemSpace.GetParent(Repo.GetPackage);
                var problemSpaceTree = ProblemSpaceTree.Create(problemSpace);

                var instantiatedTree = problemSpaceTree
                    .InstantiateSolutionPackages(parentPackage, Repo, Options.Some(solutionName))
                    .InstantiateSolutionElements(Repo);
                instantiatedTree.InstantiateSolutionConnectors(Repo);
                instantiatedTree.CreateSolutionDiagrams(Repo);
            });

            return Unit.Instance;
        }

        public bool CanExecute(ModelEntity.Package p)
        {
            return true;
        }
    }
}
