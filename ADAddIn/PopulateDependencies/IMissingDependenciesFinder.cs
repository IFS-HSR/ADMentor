using AdAddIn.ADTechnology.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdAddIn.PopulateDependencies
{
    public interface IMissingDependenciesFinder
    {
        /// <summary>
        /// Returns a dependency tree consisting of elements and connectors that can be used to instantiate
        /// additional dependencies for <c>e</c>. Additionally, this method returns a list of elements
        /// that are part of the dependency tree but have no instance that is related to <c>e</c>.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        Tuple<DependencyTree.Node, IEnumerable<EA.Element>> FindMissingDependencies(ProblemOccurrence po);
    }
}
