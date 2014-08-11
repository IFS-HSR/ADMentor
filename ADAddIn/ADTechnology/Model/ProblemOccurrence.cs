using EAAddInFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.ADTechnology.Model
{
    public class ProblemOccurrence : EAEntity<ProblemOccurrence>
    {
        private ProblemOccurrence(EA.Repository repo, EA.Element e) : base(repo, e) { }

        public Option<Problem> GetProblem()
        {
            return Classifier.SelectMany(classifier => Problem.Create(Repo, classifier));
        }

        public static Option<ProblemOccurrence> Create(EA.Repository repo, EA.Element e)
        {
            return Create(ElementStereotypes.ProblemOccurrence, e, () => new ProblemOccurrence(repo, e));
        }
    }
}
