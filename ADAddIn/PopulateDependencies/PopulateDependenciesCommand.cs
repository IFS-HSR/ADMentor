using AdAddIn.ADTechnology;
using EAAddInFramework;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.PopulateDependencies
{
    class PopulateDependenciesCommand : ICommand<EA.Element, EntityModified>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IReadableAtom<EA.Repository> Repo;

        private readonly IDependencySelector Selector;

        public PopulateDependenciesCommand(IReadableAtom<EA.Repository> repo, IDependencySelector selector)
        {
            Repo = repo;
            Selector = selector;
        }

        public EntityModified Execute(EA.Element element)
        {
            var modified =
                from currentDiagram in GetCurrentDiagramContaining(element)
                from solution in SolutionInstantiationTree.Create(Repo.Val, element)
                from selectedSolution in Selector.GetSelectedDependencies(solution)
                let instantiatedSolution = SolutionInstantiationTree.InstantiateSelectedItems(Repo.Val, element.FindPackage(Repo.Val), selectedSolution)
                let _ = SolutionInstantiationTree.CreateDiagramElements(Repo.Val, currentDiagram, instantiatedSolution)
                select EntityModified.Modified;

            return modified.GetOrElse(EntityModified.NotModified);
        }

        public Boolean CanExecute(EA.Element element)
        {
            return GetCurrentDiagramContaining(element).IsDefined;
        }

        private Option<EA.Diagram> GetCurrentDiagramContaining(EA.Element element)
        {
            return from diagram in Repo.Val.GetCurrentDiagram().AsOption()
                   where diagram.DiagramObjects.Cast<EA.DiagramObject>().Any(o => o.ElementID == element.ElementID)
                   select diagram;
        }

        public ICommand<Option<ContextItem>, object> AsMenuCommand()
        {
            return this.Adapt<Option<ContextItem>, EA.Element, object>(
                contextItem => from ci in contextItem from e in Repo.Val.TryGetElement(ci.Guid) select e);
        }

        public ICommand<Func<EA.Element>, EntityModified> AsElementCreatedHandler()
        {
            return this.Adapt<Func<EA.Element>, EA.Element, EntityModified>(
                getElement =>
                {
                    var element = getElement();
                    if (element.IsNew())
                    {
                        return Options.Some(element);
                    }
                    else
                    {
                        return Options.None<EA.Element>();
                    }
                });
        }
    }
}
