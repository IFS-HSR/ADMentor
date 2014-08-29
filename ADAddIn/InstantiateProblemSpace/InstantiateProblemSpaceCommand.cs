using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using AdAddIn.PopulateDependencies;
using EAAddInFramework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.InstantiateProblemSpace
{
    public class InstantiateProblemSpaceCommand : ICommand<EA.Package, Unit>
    {
        private readonly PackageRepository PackageRepo;
        private readonly InstantiateSolutionForm SolutionNameForm;
        private readonly ElementRepository ElementRepo;
        private readonly DiagramRepository DiagramRepo;

        public InstantiateProblemSpaceCommand(PackageRepository packageRepo,
            ElementRepository elementRepo, DiagramRepository diagramRepo, InstantiateSolutionForm solutionNameForm)
        {
            PackageRepo = packageRepo;
            ElementRepo = elementRepo;
            DiagramRepo = diagramRepo;
            SolutionNameForm = solutionNameForm;
        }

        public Unit Execute(EA.Package problemSpace)
        {
            var requiredData = from parentPackage in PackageRepo.GetPackage(problemSpace.ParentID)
                               let problemSpaceTree = ProblemSpaceTree.Create(problemSpace)
                               from solutionName in SolutionNameForm.GetSolutionName(problemSpaceTree)
                               select Tuple.Create(problemSpaceTree, parentPackage, solutionName);

            requiredData.ForEach((problemSpaceTree, parentPackage, solutionName) =>
            {
                var instantiatedTree = problemSpaceTree
                    .InstantiateSolutionPackages(PackageRepo, parentPackage)
                    .InstantiateSolutionElements(ElementRepo);
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
                solutionPackage.Name = solutionName;
                solutionPackage.Update();
            });
        }

        public bool CanExecute(EA.Package p)
        {
            return PackageRepo.GetPackage(p.ParentID).IsDefined;
        }

        public ICommand<Option<ContextItem>, object> AsMenuCommand()
        {
            return this.Adapt<Option<ContextItem>, EA.Package, object>(
                contextItem => from ci in contextItem from p in PackageRepo.GetPackage(ci.Guid) select p);
        }
    }
}
