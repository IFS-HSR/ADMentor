using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace EAAddInFramework
{
    public interface ICustomDetailViewComponent
    {
        CustomDetailViewResult DisplayCustomElementDetails(EA.Element e, bool isNew);
    }

    public sealed class CustomDetailViewResult
    {
        public CustomDetailViewResult(bool entityChanged = false, bool suppressDefaultDialog = false)
        {
            EntityChanged = entityChanged;
            SuppressDefaultDialog = suppressDefaultDialog;
        }
        public bool EntityChanged { get; private set; }
        public bool SuppressDefaultDialog { get; private set; }
    }

    class CustomDetailViewHandler
    {
        private IReadableAtom<EA.Repository> repository;
        private IList<ICustomDetailViewComponent> customDetailViewComponents = new List<ICustomDetailViewComponent>();

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public CustomDetailViewHandler(IReadableAtom<EA.Repository> repository){
            this.repository = repository;
        }

        public void Register(ICustomDetailViewComponent component)
        {
            customDetailViewComponents.Add(component);
        }

        public bool CallElementDetailViews(Func<EA.Element> getElement, bool isNew)
        {
            if (customDetailViewComponents.Count > 0)
            {
                var element = getElement();

                var res = customDetailViewComponents
                    .Aggregate(new CustomDetailViewResult(), (acc, component) =>
                    {
                        var componentResult = component.DisplayCustomElementDetails(element, true);
                        return new CustomDetailViewResult(acc.EntityChanged || componentResult.EntityChanged,
                            acc.SuppressDefaultDialog || componentResult.SuppressDefaultDialog);
                    });

                if (res.SuppressDefaultDialog)
                {
                    logger.Debug("Intercept display of detail view of element {0}", element.ElementGUID);
                    repository.Val.SuppressEADialogs = true;
                }
                return res.EntityChanged;
            }
            else
            {
                return false;
            }
        }
    }
}
