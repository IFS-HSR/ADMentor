using AdAddIn.Model;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.DataAccess.ADModel
{
    public class ADIssueRepository : IIssueRepository
    {
        private readonly IReadableAtom<EA.Repository> repository;

        public ADIssueRepository(IReadableAtom<EA.Repository> repository)
        {
            this.repository = repository;
        }

        public IEnumerable<Issue> GetAllIn(int diagramId)
        {
            return from diagram in Options.Try(() => repository.Val.GetDiagramByID(diagramId))
                   from obj in diagram.DiagramObjects.Cast<EA.DiagramObject>()
                   from issue in GetById(obj.ElementID)
                   select issue;
        }

        public Option<Issue> GetById(int id)
        {
            return from e in Options.Try(() => repository.Val.GetElementByID(id))
                   where e.Stereotype == ADStereotypes.Issue
                   select new Issue(id: e.ElementID, name: e.Name);
        }
    }
}
