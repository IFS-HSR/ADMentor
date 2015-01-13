using ADMentor.DataAccess;
using EAAddInBase;
using EAAddInBase.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace ADMentor.Validation
{
    public class ElementNotUsedDiagramRule : ValidationRule
    {
        private readonly AdRepository Repo;

        private readonly Atom<IImmutableSet<int>> ElementIdsWithDiagrams = new Atom<IImmutableSet<int>>(ImmutableHashSet.Create<int>());

        public ElementNotUsedDiagramRule(String category, AdRepository repo)
            : base(category)
        {
            Repo = repo;
        }

        public override void Prepare()
        {
            var allElementIds = (from package in Repo.AllPackages
                                from diagram in package.Diagrams
                                from obj in diagram.Objects
                                select obj.EaObject.ElementID).ToImmutableHashSet();

            ElementIdsWithDiagrams.Exchange(allElementIds, GetType());
        }

        public override Option<ValidationMessage> Execute(ModelEntity e)
        {
            return from element in e.TryCast<ModelEntity.Element>()
                   where !ElementIdsWithDiagrams.Val.Contains(element.Id)
                   select ValidationMessage.Information("Element not used in a Diagram");
        }
    }
}
