using ADMentor.ADTechnology;
using ADMentor.DataAccess;
using EAAddInBase;
using EAAddInBase.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;
using EAAddInBase.MDGBuilder;

namespace ADMentor.InstantiateProblemSpace
{
    public sealed class ProblemSpaceTree
    {
        private ProblemSpaceTree(
            ModelEntity.Package package,
            Option<ModelEntity.Package> packageInstance,
            IEnumerable<ElementInstantiation> instantiations,
            IEnumerable<ModelEntity.Diagram> diagrams,
            IEnumerable<ProblemSpaceTree> children)
        {
            Package = package;
            PackageInstance = packageInstance;
            ElementInstantiations = instantiations.Run();
            Diagrams = diagrams.Run();
            Children = children.Run();
        }

        public ModelEntity.Package Package { get; private set; }

        public Option<ModelEntity.Package> PackageInstance { get; private set; }

        public IEnumerable<ElementInstantiation> ElementInstantiations { get; private set; }

        public IEnumerable<ModelEntity.Diagram> Diagrams { get; private set; }

        public IEnumerable<ProblemSpaceTree> Children { get; private set; }

        public static ProblemSpaceTree Create(ModelEntity.Package problemSpacePackage)
        {
            var instantiations = from element in problemSpacePackage.Elements
                                 where element is OptionEntity || element is Problem
                                 select new ElementInstantiation(element);
            var diagrams = from diagram in problemSpacePackage.Diagrams
                           where diagram.Is(DiagramTypes.ProblemSpace)
                           select diagram;
            var children = from childPackage in problemSpacePackage.Packages
                           let childTree = Create(childPackage)
                           where childTree.AllInstantiations().Any()
                           select childTree;

            return new ProblemSpaceTree(problemSpacePackage, Options.None<ModelEntity.Package>(), instantiations, diagrams, children);
        }

        public IEnumerable<ElementInstantiation> AllInstantiations()
        {
            // if this package has been instantiated, we also add the instantiation of its
            // associated element (if any) to the instantiations. This allows us to also create
            // connectors and diagram objects for instantiated packages.
            var packageElementInstantiation = from element in Package.AssociatedElement
                                              from instance in PackageInstance
                                              select new ElementInstantiation(element, instance.AssociatedElement);

            return Children.Aggregate(ElementInstantiations, (acc, child) =>
            {
                return acc.Concat(child.AllInstantiations());
            }).Concat(packageElementInstantiation);
        }

        public ProblemSpaceTree InstantiateSolutionPackages(Option<ModelEntity.Package> parentPackage, ModelEntityRepository repo, Option<String> name)
        {
            var solutionPackageName = name.GetOrElse(Package.Name);
            var solutionPackage = repo.CreatePackage(solutionPackageName, parentPackage, SolutionSpace.SolutionSpacePackage.AsOption());

            var children = Children.Select(c => c.InstantiateSolutionPackages(solutionPackage.AsOption(), repo, Options.None<String>()));

            return new ProblemSpaceTree(Package, Options.Some(solutionPackage), ElementInstantiations, Diagrams, children);
        }

        public ProblemSpaceTree InstantiateSolutionElements(ModelEntityRepository repo)
        {
            var instantiations = from instantiation in ElementInstantiations
                                 select PackageInstance.Fold(
                                            (solutionPackage) =>
                                                instantiation.CreateInstanceIfMissing(repo, solutionPackage),
                                            () => instantiation);

            var children = Children.Select(c => c.InstantiateSolutionElements(repo));

            return new ProblemSpaceTree(Package, PackageInstance, instantiations, Diagrams, children);
        }

        public void InstantiateSolutionConnectors(ModelEntityRepository repo)
        {
            var allInstantiations = AllInstantiations().Run();
            // connections between new instances
            var connections = from instantiation in allInstantiations
                              from source in instantiation.Instance
                              from connector in instantiation.Element.Connectors
                              where connector.EaObject.ClientID == instantiation.Element.Id
                              from connectorStype in connector.GetStereotype(ADTechnology.Technologies.AD.ConnectorStereotypes)
                              from targetInstantiation in allInstantiations
                              where connector.EaObject.SupplierID == targetInstantiation.Element.Id
                              from target in targetInstantiation.Instance
                              select Tuple.Create(source, connectorStype, target);
            
            // connections from new instances to referenced elements
            var referenceConnectionsFrom = from instantiation in allInstantiations
                                           from instance in instantiation.Instance
                                           from connector in instantiation.Element.Connectors
                                           where connector.EaObject.ClientID == instantiation.Element.Id
                                           from target in connector.Target(repo.GetElement)
                                           where !target.Is(ProblemSpace.Problem) && !target.Is(ProblemSpace.Option)
                                           select Tuple.Create(instance, connector.ReconstructStereotype(), target);

            // connections from referenced elements to new instances
            var referenceConnectionsTo = from instantiation in allInstantiations
                                         from instance in instantiation.Instance
                                         from connector in instantiation.Element.Connectors
                                         where connector.EaObject.SupplierID == instantiation.Element.Id
                                         from source in connector.Source(repo.GetElement)
                                         where !source.Is(ProblemSpace.Problem) && !source.Is(ProblemSpace.Option)
                                         select Tuple.Create(source, connector.ReconstructStereotype(), instance);

            connections
                .Concat(referenceConnectionsFrom)
                .Concat(referenceConnectionsTo)
                .ForEach((source, connectorStype, target) =>
                {
                    try
                    {
                        repo.Connect(source, target, connectorStype);
                    }
                    catch (Exception)
                    {
                        // TODO: proper error handling
                    }
                });
        }

        public void CreateSolutionDiagrams(ModelEntityRepository repo)
        {
            CreateSolutionDiagrams(repo, AllInstantiations());
        }

        private void CreateSolutionDiagrams(ModelEntityRepository repo, IEnumerable<ElementInstantiation> instantiations)
        {
            PackageInstance.Do(solutionPackage =>
            {
                Diagrams.ForEach(problemDiagram =>
                {
                    var solutionDiagram = repo.Create(problemDiagram.Name, DiagramTypes.SolutionOverview, solutionPackage, ADTechnology.Technologies.AD.ID);
                    CopyDiagramObjects(problemDiagram, solutionDiagram, instantiations);
                });
            });

            Children.ForEach(c => c.CreateSolutionDiagrams(repo, instantiations));
        }

        private void CopyDiagramObjects(ModelEntity.Diagram problemDiagram, ModelEntity.Diagram solutionDiagram, IEnumerable<ElementInstantiation> instantiations)
        {
            var solutionObjects = from problemObject in problemDiagram.Objects
                                  let instantiation = instantiations.FirstOption(i => i.Element.Id == problemObject.EaObject.ElementID)
                                  let solutionElement = instantiation.SelectMany(i => i.Instance)
                                  let elementId = solutionElement.Fold(e => e.Id, () => problemObject.EaObject.ElementID)
                                  select Tuple.Create(problemObject, elementId);

            solutionObjects.ForEach((problemObject, elementId) =>
            {
                solutionDiagram.AddObject(elementId,
                    left: problemObject.EaObject.left, right: problemObject.EaObject.right,
                    top: problemObject.EaObject.top, bottom: problemObject.EaObject.bottom);
            });
        }
    }
}
