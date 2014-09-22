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
            var requiredData = from parentPackage in problemSpace.GetParent(Repo.GetPackage)
                               let problemSpaceTree = ProblemSpaceTree.Create(problemSpace)
                               from solutionName in SolutionNameForm.GetSolutionName(problemSpaceTree)
                               select Tuple.Create(problemSpaceTree, parentPackage, solutionName);

            requiredData.ForEach((problemSpaceTree, parentPackage, solutionName) =>
            {
                Repo.SaveAllDiagrams();

                var instantiatedTree = problemSpaceTree
                    .InstantiateSolutionPackages(parentPackage)
                    .InstantiateSolutionElements(Repo);
                RenameSolutionPackage(instantiatedTree, solutionName);
                instantiatedTree.InstantiateSolutionConnectors(Repo);
                instantiatedTree.CreateSolutionDiagrams(Repo);
            });

            return Unit.Instance;
        }

        private void RenameSolutionPackage(ProblemSpaceTree instantiatedTree, string solutionName)
        {
            instantiatedTree.PackageInstance.Do(solutionPackage =>
            {
                solutionPackage.EaObject.Name = solutionName;
                solutionPackage.EaObject.Update();
            });
        }

        public bool CanExecute(ModelEntity.Package p)
        {
            return p.GetParent(Repo.GetPackage).IsDefined;
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
