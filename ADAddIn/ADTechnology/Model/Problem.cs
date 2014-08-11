using EAAddInFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.ADTechnology.Model
{
    public class Problem : EAEntity<Problem>
    {
        private Problem(EA.Repository repo, EA.Element e) : base(repo, e) { }

        public static Option<Problem> Create(EA.Repository repo, EA.Element e)
        {
            return Create(ElementStereotypes.Problem, e, () => new Problem(repo, e));
        }
    }
}
