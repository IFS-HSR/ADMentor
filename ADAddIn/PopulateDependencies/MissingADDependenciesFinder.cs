using AdAddIn.ADTechnology.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.PopulateDependencies
{
    public class MissingADDependenciesFinder : IDependenciesFinder
    {
        private readonly IReadableAtom<EA.Repository> Repo;

        public MissingADDependenciesFinder(IReadableAtom<EA.Repository> repo)
        {
            Repo = repo;
        }

        public Option<DependencyTree.Node> FindPotentialDependencies(EA.Element element)
        {
            return from po in ProblemOccurrence.Create(Repo.Val, element)
                   from problem in po.GetProblem()
                   select DependencyTree.Create(Repo.Val, problem.Element, DependencyTree.TraverseOnlyADConnectors);
        }

        public IEnumerable<EA.Element> SelectMissingDependencies(DependencyTree.Node dependencies, EA.Element element)
        {
            var existingDependencies = DependencyTree.Create(Repo.Val, element, DependencyTree.TraverseOnlyADConnectors);

            return from dep in dependencies.Elements
                   where existingDependencies.Elements.Any(e => e.ClassifierID == dep.ElementID)
                   select dep;
        }
    }
}
