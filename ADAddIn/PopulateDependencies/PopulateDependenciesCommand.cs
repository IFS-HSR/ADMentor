using AdAddIn.ADTechnology;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
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
            CollectRequiredData(element).ForEach((currentDiagram, problem) =>
            {
                var dependencies = DependencyTree.Create(Repo.Val, problem, levels: 3);
                var alreadyAddedDependencies = dependencies.Where(dependency =>
                {
                    return (from o in currentDiagram.DiagramObjects.Cast<EA.DiagramObject>()
                            from e in Repo.Val.TryGetElement(o.ElementID)
                            where e.ClassfierID == dependency.ElementID
                            select e).Count() > 0;
                });
                var selectedDependencies = Selector.GetSelectedDependencies(dependencies, alreadyAddedDependencies);
            });

            return EntityModified.NotModified;
        }

        public Boolean CanExecute(EA.Element element)
        {
            return CollectRequiredData(element).IsDefined;
        }

        private Option<Tuple<EA.Diagram, EA.Element>> CollectRequiredData(EA.Element element)
        {
            return from diagram in Repo.Val.GetCurrentDiagram().AsOption()
                   where diagram.DiagramObjects.Cast<EA.DiagramObject>().Any(o => o.ElementID == element.ElementID)
                   from problem in GetProblemForClassifier(element)
                   select Tuple.Create(diagram, problem);
        }

        private Option<EA.Element> GetProblemForClassifier(EA.Element element)
        {
            return from classifier in Repo.Val.TryGetElement(element.ClassifierID)
                   let dependencyTree = DependencyTree.Create(Repo.Val, classifier, DependencyTree.TraverseFromAlternativeToProblem)
                   from problem in dependencyTree.FirstOption(e => e.Is(ElementStereotypes.Problem))
                   select problem;
        }

        public ICommand<Option<ContextItem>, object> AsMenuCommand()
        {
            return this.Adapt<Option<ContextItem>, EA.Element, object>(
                contextItem => from ci in contextItem from e in Repo.Val.TryGetElement(ci.Guid) select e);
        }

        public ICommand<Func<EA.Element>, EntityModified> AsDetailViewCommand()
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
