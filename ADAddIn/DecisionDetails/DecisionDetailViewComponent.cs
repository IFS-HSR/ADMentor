using EAAddInFramework;
using AdAddIn.DataAccess;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.DecisionDetails
{
    public class DecisionDetailViewComponent : ICustomDetailViewComponent
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly DecisionDetailView view;
        private readonly IDecisionRepository decisions;
        private readonly IAlternativeRepository alternatives;
        private readonly IIssueRepository issues;

        public DecisionDetailViewComponent(DecisionDetailView view, IDecisionRepository decisions, IAlternativeRepository alternatives, IIssueRepository issues)
        {
            this.view = view;
            this.decisions = decisions;
            this.alternatives = alternatives;
            this.issues = issues;
        }

        public CustomDetailViewResult DisplayCustomElementDetails(EA.Element element, bool isNew)
        {
            return decisions.GetById(element.ElementID).Match(
                decision =>
                {
                    logger.Debug("Display custom decision detail view for {0}", decision);
                    view.GetModifications(decision, alternatives.GetAll(), issues.GetAllIn(0)).Do(
                        newDecision =>
                        {
                            logger.Debug("Decision has been modified by user");
                            decisions.Update(newDecision);
                        }
                    );
                    return new CustomDetailViewResult(entityChanged: true);
                },
                () =>
                {
                    return new CustomDetailViewResult();
                });
        }
    }
}
