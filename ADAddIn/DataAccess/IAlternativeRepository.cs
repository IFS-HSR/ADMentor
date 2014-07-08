using AdAddIn.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.DataAccess
{
    public interface IAlternativeRepository
    {
        IEnumerable<Alternative> GetAll();
        Option<Alternative> GetById(int id);
    }
}
