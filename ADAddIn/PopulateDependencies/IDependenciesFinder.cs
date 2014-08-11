using AdAddIn.ADTechnology.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.PopulateDependencies
{
    public interface IDependenciesFinder
    {
        /// <summary>
        /// Returns a dependency tree consisting of elements and connectors that can be used to instantiate
        /// additional dependencies for <c>e</c>.
        /// </summary>
        Option<DependencyTree.Node> FindPotentialDependencies(EA.Element element);

        IEnumerable<EA.Element> SelectMissingDependencies(DependencyTree.Node dependencies, EA.Element element);
    }
}
