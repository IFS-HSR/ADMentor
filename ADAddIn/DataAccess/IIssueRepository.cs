using AdAddIn.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.DataAccess
{
    public interface IIssueRepository
    {
        IEnumerable<Issue> GetAllIn(int diagramId);
        Option<Issue> GetById(int id);
    }
}
