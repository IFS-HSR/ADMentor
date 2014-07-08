using AdAddIn.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace AdAddIn.DataAccess
{
    public interface IDecisionRepository
    {
        Option<Decision> GetById(int id);
        void Update(Decision decision);
    }
}
