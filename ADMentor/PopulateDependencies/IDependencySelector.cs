using ADMentor.DataAccess;
using EAAddInBase.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace ADMentor.PopulateDependencies
{
    public interface IDependencySelector
    {
        Option<LabeledTree<ElementInstantiation, ModelEntity.Connector>> GetSelectedDependencies(
            LabeledTree<ElementInstantiation, ModelEntity.Connector> availableDependencies);
    }
}
