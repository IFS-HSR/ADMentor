using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using AdAddIn.PopulateDependencies;
using EAAddInFramework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.InstantiateProblemSpace
{
    public class InstantiateProblemSpaceCommand : ICommand<EA.Package, Unit>
    {
        private readonly PackageRepository PackageRepo;
        private readonly SolutionNameForm SolutionNameForm;
        private readonly ElementRepository ElementRepo;
        private readonly DiagramRepository DiagramRepo;

        public InstantiateProblemSpaceCommand(PackageRepository packageRepo,
            ElementRepository elementRepo, DiagramRepository diagramRepo, SolutionNameForm solutionNameForm)
        {
            PackageRepo = packageRepo;
            ElementRepo = elementRepo;
            DiagramRepo = diagramRepo;
            SolutionNameForm = solutionNameForm;
        }

        public Unit Execute(EA.Package problemSpace)
        {
            var requiredData = from parentPackage in PackageRepo.GetPackage(problemSpace.ParentID)
                               from solutionName in SolutionNameForm.GetSolutionName()
                               select Tuple.Create(parentPackage, solutionName);

            requiredData.ForEach((parentPackage, solutionName) =>
            {
                var solutionPackage = PackageRepo.Create(parentPackage, solutionName);
                var instantiations = InstantiateElements(problemSpace, solutionPackage);
                InstantiateConnectors(instantiations);
                CopyDiagrams(problemSpace, solutionPackage, instantiations);
            });

            return Unit.Instance;
        }

        private IImmutableList<ElementInstantiation> InstantiateElements(EA.Package problemSpace, EA.Package solutionPackage)
        {
            var instantiations = problemSpace.Elements().Aggregate(ImmutableList.Create<ElementInstantiation>(), (acc, problemItem) =>
            {
                var solutionItem = ElementRepo.Instanciate(problemItem, solutionPackage);
                return acc.Add(new ElementInstantiation(problemItem, solutionItem));
            });

            return problemSpace.Packages().Aggregate(instantiations, (acc, problemChild) =>
            {
                var solutionChild = PackageRepo.Create(solutionPackage, problemChild.Name);
                return acc.AddRange(InstantiateElements(problemChild, solutionChild));
            });
        }

        private void InstantiateConnectors(IImmutableList<ElementInstantiation> instantiations)
        {
            Func<EA.Connector, Option<EA.Element>> findTargetEnd = (connector) =>
            {
                return (from instantiation in instantiations
                        from target in instantiation.Instance
                        where instantiation.Element.ElementID == connector.SupplierID
                        select target).FirstOption();
            };

            var connections = from instantiation in instantiations
                              from source in instantiation.Instance
                              from connector in instantiation.Element.Connectors.Cast<EA.Connector>()
                              where connector.ClientID == instantiation.Element.ElementID
                              from connectorStype in ElementRepo.GetStereotype(connector)
                              from target in findTargetEnd(connector)
                              select Tuple.Create(source, connectorStype, target);

            connections.ForEach((source, connectorStype, target) =>
            {
                connectorStype.Create(source, target);
            });
        }

        private void CopyDiagrams(EA.Package problemSpace, EA.Package solutionPackage, IImmutableList<ElementInstantiation> instantiations)
        {
            problemSpace.Diagrams().ForEach(problemDiagram =>
            {
                var solutionDiagram = DiagramRepo.Create(Diagrams.SolutionOverview, solutionPackage, problemDiagram.Name);
                CopyDiagramObjects(problemDiagram, solutionDiagram, instantiations);
            });

            problemSpace.Packages().ForEach(problemPkg =>
            {
                solutionPackage.Packages().FirstOption(p => p.Name == problemPkg.Name).Do(solutionPkg =>
                {
                    CopyDiagrams(problemPkg, solutionPkg, instantiations);
                });
            });
        }

        private void CopyDiagramObjects(EA.Diagram problemDiagram, EA.Diagram solutionDiagram, IImmutableList<ElementInstantiation> instantiations)
        {
            var newObjects = from problemObject in problemDiagram.DiagramObjects()
                             join instantiation in instantiations on problemObject.ElementID equals instantiation.Element.ElementID
                             from solutionElement in instantiation.Instance
                             select Tuple.Create(problemObject, solutionElement);

            newObjects.ForEach((problemObject, solutionElement) =>
            {
                DiagramRepo.AddToDiagram(solutionDiagram, solutionElement,
                    left: problemObject.left, right: problemObject.right, top: problemObject.top, bottom: problemObject.bottom);
            });
        }

        public bool CanExecute(EA.Package p)
        {
            return PackageRepo.GetPackage(p.ParentID).IsDefined;
        }

        public ICommand<Option<ContextItem>, object> AsMenuCommand()
        {
            return this.Adapt<Option<ContextItem>, EA.Package, object>(
                contextItem => from ci in contextItem from p in PackageRepo.GetPackage(ci.Guid) select p);
        }
    }
}
