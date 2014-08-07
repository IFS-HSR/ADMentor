using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.PopulateDependencies
{
    public interface IDependencySelector
    {
        Option<DependencyTree.Node> GetSelectedDependencies(DependencyTree.Node availableDependencies, IEnumerable<EA.Element> alreadyAddedElements);
    }
}
