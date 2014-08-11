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
        Option<LabeledTree<EA.Element, EA.Connector>> GetSelectedDependencies(LabeledTree<EA.Element, EA.Connector> availableDependencies, IEnumerable<EA.Element> alreadyAddedElements);
    }
}
