using AdAddIn.ADTechnology;
using EAAddInFramework;
using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdAddIn.Components
{
    public class PopulateDependenciesOnNewOccurrences : ICustomDetailViewComponent
    {

        public CustomDetailViewResult DisplayCustomElementDetails(EA.Element e, bool isNew)
        {
            if (isNew && e.Is(ElementStereotypes.ProblemOccurrence) && e.ClassifierID != 0)
            {
                DisplayPopulateDependenciesView(e);
                return new CustomDetailViewResult(entityChanged: true, suppressDefaultDialog: false);
            }
            else
            {
                return new CustomDetailViewResult();
            }
        }

        private void DisplayPopulateDependenciesView(EA.Element e)
        {
            // todo
        }
    }
}
