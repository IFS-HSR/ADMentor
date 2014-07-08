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
    public class ADDecisionRepository : IDecisionRepository
    {
        private readonly IReadableAtom<EA.Repository> repository;

        public ADDecisionRepository(IReadableAtom<EA.Repository> repository)
        {
            this.repository = repository;
        }

        public Option<Decision> GetById(int id)
        {
            return from e in Options.Try(() => repository.Val.GetElementByID(id))
                   where e.Stereotype == ADStereotypes.Decision
                   select new Decision(
                           id: e.ElementID,
                           name: e.Name,
                           instantiatesAlternative: e.ClassifierID.AsOption()
                       );
        }

        public void Update(Decision decision)
        {
            Options.Try(() => repository.Val.GetElementByID(decision.ID))
                .Do(e =>
                {
                    e.Name = decision.Name;
                    e.ClassifierID = decision.InstantiatesAlternative.GetOrDefault();

                    decision.AddressesIssue.Do(issueId =>
                    {
                        var connector = (EA.Connector)e.Connectors.AddNew("c", "Association");
                        connector.SupplierID = issueId;
                        connector.Update();
                        e.Connectors.Refresh();
                    });

                    e.Update();
                    repository.Val.AdviseElementChange(e.ElementID);
                });
        }
    }
}
