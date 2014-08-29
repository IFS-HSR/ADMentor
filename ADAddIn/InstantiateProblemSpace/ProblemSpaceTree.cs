using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using AdAddIn.PopulateDependencies;
using EAAddInFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.InstantiateProblemSpace
{
    public class ProblemSpaceTree
    {
        private ProblemSpaceTree(
            EA.Package package,
            Option<EA.Package> packageInstance,
            IEnumerable<ElementInstantiation> instantiations,
            IEnumerable<EA.Diagram> diagrams,
            IEnumerable<ProblemSpaceTree> children)
        {
            Package = package;
            PackageInstance = packageInstance;
            ElementInstantiations = instantiations.Run();
            Diagrams = diagrams.Run();
            Children = children.Run();
        }

        public EA.Package Package { get; private set; }

        public Option<EA.Package> PackageInstance { get; private set; }

        public IEnumerable<ElementInstantiation> ElementInstantiations { get; private set; }

        public IEnumerable<EA.Diagram> Diagrams { get; private set; }

        public IEnumerable<ProblemSpaceTree> Children { get; private set; }

        public static ProblemSpaceTree Create(EA.Package problemSpacePackage)
        {
            var instantiations = from element in problemSpacePackage.Elements()
                                 where element.Is(ElementStereotypes.Option) || element.Is(ElementStereotypes.Problem)
                                 select new ElementInstantiation(element);
            var diagrams = from diagram in problemSpacePackage.Diagrams()
                           where diagram.Is(DiagramTypes.ProblemSpace)
                           select diagram;
            var children = from childPackage in problemSpacePackage.Packages()
                           let childTree = Create(childPackage)
                           where childTree.AllInstantiations().Count() > 0
                           select childTree;

            return new ProblemSpaceTree(problemSpacePackage, Options.None<EA.Package>(), instantiations, diagrams, children);
        }

        public IEnumerable<ElementInstantiation> AllInstantiations()
        {
            return Children.Aggregate(ElementInstantiations, (acc, child) =>
            {
                return acc.Concat(child.AllInstantiations());
            });
        }

        public ProblemSpaceTree InstantiateSolutionPackages(PackageRepository packageRepo, EA.Package parentPackage)
        {
            var solutionPackage = packageRepo.Create(parentPackage, Package.Name);
            var children = Children.Select(c => c.InstantiateSolutionPackages(packageRepo, solutionPackage));

            return new ProblemSpaceTree(Package, Options.Some(solutionPackage), ElementInstantiations, Diagrams, children);
        }

        public ProblemSpaceTree InstantiateSolutionElements(ElementRepository elementRepo)
        {
            var instantiations = from instantiation in ElementInstantiations
                                 select PackageInstance.Match(
                                            (solutionPackage) =>
                                                instantiation.CreateInstanceIfMissing(elementRepo, solutionPackage),
                                            () => instantiation);

            var children = Children.Select(c => c.InstantiateSolutionElements(elementRepo));

            return new ProblemSpaceTree(Package, PackageInstance, instantiations, Diagrams, children);
        }

        public void InstantiateSolutionConnectors(ElementRepository elementRepo)
        {
            var allInstantiations = AllInstantiations();
            var connections = from instantiation in allInstantiations
                              from source in instantiation.Instance
                              from connector in instantiation.Element.Connectors()
                              where connector.ClientID == instantiation.Element.ElementID
                              from connectorStype in elementRepo.GetStereotype(connector)
                              join targetInstantiation in allInstantiations
                                on connector.SupplierID equals targetInstantiation.Element.ElementID
                              from target in targetInstantiation.Instance
                              select Tuple.Create(source, connectorStype, target);

            connections.ForEach((source, connectorStype, target) =>
            {
                connectorStype.Create(source, target);
            });
        }

        public void CreateSolutionDiagrams(DiagramRepository diagramRepo)
        {
            CreateSolutionDiagrams(diagramRepo, AllInstantiations());
        }

        private void CreateSolutionDiagrams(DiagramRepository diagramRepo, IEnumerable<ElementInstantiation> instantiations)
        {
            PackageInstance.Do(solutionPackage =>
            {
                Diagrams.ForEach(problemDiagram =>
                {
                    var solutionDiagram = diagramRepo.Create(DiagramTypes.SolutionOverview, solutionPackage, problemDiagram.Name);
                    CopyDiagramObjects(diagramRepo, problemDiagram, solutionDiagram, instantiations);
                });
            });

            Children.ForEach(c => c.CreateSolutionDiagrams(diagramRepo, instantiations));
        }

        private void CopyDiagramObjects(DiagramRepository diagramRepo, EA.Diagram problemDiagram, EA.Diagram solutionDiagram, IEnumerable<ElementInstantiation> instantiations)
        {
            var newObjects = from problemObject in problemDiagram.DiagramObjects()
                             join instantiation in instantiations
                               on problemObject.ElementID equals instantiation.Element.ElementID
                             from solutionElement in instantiation.Instance
                             select Tuple.Create(problemObject, solutionElement);

            newObjects.ForEach((problemObject, solutionElement) =>
            {
                diagramRepo.AddToDiagram(solutionDiagram, solutionElement,
                    left: problemObject.left, right: problemObject.right, top: problemObject.top, bottom: problemObject.bottom);
            });
        }
    }
}
