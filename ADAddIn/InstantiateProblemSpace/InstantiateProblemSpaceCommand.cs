using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using AdAddIn.PopulateDependencies;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.InstantiateProblemSpace
{
    public class InstantiateProblemSpaceCommand : ICommand<ModelEntity.Package, Unit>
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
                               var problemSpaceTree = ProblemSpaceTree.Create(problemSpace);

            SolutionNameForm.GetSolutionName(problemSpaceTree).Do(solutionName =>
            {
                Repo.SaveAllDiagrams();

                var parentPackage = problemSpace.GetParent(Repo.GetPackage);

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

        public ICommand<Option<ModelEntity>, object> AsMenuCommand()
        {
            return this.Adapt(
                (Option<ModelEntity> contextItem) =>
                {
                    return from ci in contextItem
                           from p in ci.Match<ModelEntity.Package>()
                           select p;
                });
        }
    }
}
