using AdAddIn.ADTechnology.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.PopulateDependencies
{
    public class MissingADDependenciesFinder : IMissingDependenciesFinder
    {
        private readonly IReadableAtom<EA.Repository> Repo;

        public MissingADDependenciesFinder(IReadableAtom<EA.Repository> repo)
        {
            Repo = repo;
        }

        public Tuple<DependencyTree.Node, IEnumerable<EA.Element>> FindMissingDependencies(ProblemOccurrence po)
        {
            throw new NotImplementedException();
        }
    }
}
