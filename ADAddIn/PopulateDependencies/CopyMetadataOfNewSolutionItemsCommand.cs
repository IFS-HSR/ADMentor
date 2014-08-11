using AdAddIn.ADTechnology;
using AdAddIn.ADTechnology.Model;
using EAAddInFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.PopulateDependencies
{
    public class CopyMetadataOfNewSolutionItemsCommand : ICommand<Func<EA.Element>, EntityModified>
    {
        private readonly IReadableAtom<EA.Repository> Repo;

        public CopyMetadataOfNewSolutionItemsCommand(IReadableAtom<EA.Repository> repo)
        {
            Repo = repo;
        }

        public EntityModified Execute(Func<EA.Element> getElement)
        {
            var element = getElement();

            return GetClassifier(element).Select(classifier =>
            {
                element.Notes = classifier.Notes;
                element.Update();

                return EntityModified.Modified;
            }).GetOrElse(EntityModified.NotModified);
        }

        public bool CanExecute(Func<EA.Element> getElement)
        {
            return true;
        }

        private Option<EA.Element> GetClassifier(EA.Element e)
        {
            return from po in ProblemOccurrence.Create(Repo.Val, e)
                   from p in po.GetProblem()
                   select p.Element;
        }
    }
}
