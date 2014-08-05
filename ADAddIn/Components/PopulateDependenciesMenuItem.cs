using AdAddIn.ADTechnology;
using EAAddInFramework;
using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.Components
{
    //public class PopulateDependenciesMenuItem : MenuItem
    //{
    //    private IReadableAtom<EA.Repository> Repository;

    //    public PopulateDependenciesMenuItem(IReadableAtom<EA.Repository> repo)
    //        : base("Populate Dependencies")
    //    {
    //        this.Repository = repo;
    //    }

    //    public override bool IsVisible(Utils.Option<ContextItem> contextItem)
    //    {
    //        return contextItem.IsDefined;
    //    }

    //    public override bool IsEnabled(Utils.Option<ContextItem> contextItem)
    //    {
    //        return (from ci in contextItem
    //                from e in Repository.Val.GetElementByGuid(ci.Guid).AsOption()
    //                select e.Is(ElementStereotypes.ProblemOccurrence)).GetOrElse(false);
    //    }

    //    public override void OnClick(Utils.Option<ContextItem> contextItem)
    //    {
            
    //    }
    //}
}
