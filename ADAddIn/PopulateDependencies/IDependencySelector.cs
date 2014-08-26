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
        Option<LabeledTree<SolutionInstantiation, EA.Connector>> GetSelectedDependencies(LabeledTree<SolutionInstantiation, EA.Connector> availableDependencies);

        Option<DirectedLabeledGraph<SolutionInstantiation, EA.Connector>> GetSelectedDependencies(
            DirectedLabeledGraph<SolutionInstantiation, EA.Connector> availableDependencies);
    }
}
