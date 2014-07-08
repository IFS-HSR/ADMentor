using AdAddIn.Model;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace AdAddIn.DataAccess.ADModel
{
    public class ADAlternativeRepository : IAlternativeRepository
    {
        private readonly IReadableAtom<EA.Repository> repository;

        public ADAlternativeRepository(IReadableAtom<EA.Repository> repository)
        {
            this.repository = repository;
        }

        public IEnumerable<Alternative> GetAll()
        {
            var collector = Collectors
                .Create(new List<Alternative>())
                .CollectElementsWith((alternatives, e) =>
                {
                    GetById(e.ElementID).Do(alternative => alternatives.Add(alternative));
                    return alternatives;
                });
            repository.Val.Accept(collector);
            return collector.Result;
        }

        public Option<Alternative> GetById(int id)
        {
            return from e in Options.Try(() => repository.Val.GetElementByID(id))
                   from a in ToAlternative(e)
                   select a;
        }

        public Option<Alternative> ToAlternative(EA.Element element)
        {
            return from e in element.AsOption()
                   where e.Stereotype == ADStereotypes.Alternative
                   select new Alternative(id: e.ElementID, name: e.Name, alternativeForIssue: default(int));
        }
    }
}
