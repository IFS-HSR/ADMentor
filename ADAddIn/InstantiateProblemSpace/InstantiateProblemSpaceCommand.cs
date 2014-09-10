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
        private readonly PackageRepository PackageRepo;
        private readonly InstantiateSolutionForm SolutionNameForm;
        private readonly ElementRepository ElementRepo;
        private readonly DiagramRepository DiagramRepo;
        private readonly ModelEntityRepository Repo;

        public InstantiateProblemSpaceCommand(ModelEntityRepository repo,
            PackageRepository packageRepo,
            ElementRepository elementRepo, DiagramRepository diagramRepo, InstantiateSolutionForm solutionNameForm)
        {
            Repo = repo;
            PackageRepo = packageRepo;
            ElementRepo = elementRepo;
            DiagramRepo = diagramRepo;
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
                var instantiatedTree = problemSpaceTree
                    .InstantiateSolutionPackages(parentPackage)
                    .InstantiateSolutionElements(Repo);
                RenameSolutionPackage(instantiatedTree, solutionName);
                instantiatedTree.InstantiateSolutionConnectors(ElementRepo);
                instantiatedTree.CreateSolutionDiagrams(DiagramRepo);
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

        public ICommand<Option<ContextItem>, object> AsMenuCommand()
        {
            return this.Adapt(
                (Option<ContextItem> contextItem) =>
                {
                    return from ci in contextItem
                           from p in Repo.GetPackage(ci.Guid)
                           select p;
                });
        }
    }
}
