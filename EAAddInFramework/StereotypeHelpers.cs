using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace EAAddInFramework
{
    public static class StereotypeHelpers
    {
        public static EA.Element Create(this ElementStereotype stereotype, EA.Package package, String name)
        {
            var e = package.Elements.AddNew(name, stereotype.Type.Name) as EA.Element;

            e.Stereotype = stereotype.Name;

            try
            {
                e.Update();
            }
            catch (COMException ce)
            {
                throw new ApplicationException(e.GetLastError(), ce);
            }

            package.Elements.Refresh();

            return e;
        }

        public static EA.Connector Create(this ConnectorStereotype stereotype, EA.Element source, EA.Element target, String name = "")
        {
            var c = source.Connectors.AddNew(name, stereotype.Type.Name) as EA.Connector;

            c.Stereotype = stereotype.Name;
            c.SupplierID = target.ElementID;

            try
            {
                c.Update();
            }
            catch (COMException ce)
            {
                throw new ApplicationException(c.GetLastError(), ce);
            }

            SpecifyComposition(stereotype, c);
            SpecifyNavigateability(stereotype, c);

            source.Connectors.Refresh();
            target.Connectors.Refresh();

            return c;
        }

        private static void SpecifyComposition(ConnectorStereotype stereotype, EA.Connector c)
        {
            stereotype.CompositionKind.Do(compositionKind =>
            {
                var end =
                    compositionKind.End == CompositionKind.CompositionEnd.Source ? c.ClientEnd.AsOption() :
                    compositionKind.End == CompositionKind.CompositionEnd.Target ? c.SupplierEnd.AsOption() :
                    Options.None<EA.ConnectorEnd>();
                end.Do(e =>
                {
                    e.Aggregation = (int)compositionKind.Type;
                    e.Update();
                });
            });
        }

        private static void SpecifyNavigateability(ConnectorStereotype stereotype, EA.Connector c)
        {
            var direction = stereotype.Direction.GetOrElse((stereotype.Type as ConnectorType).DefaultDirection);
            if (c.ClientEnd.Navigable != direction.SourceNavigateability.Name)
            {
                c.ClientEnd.Navigable = direction.SourceNavigateability.Name;
                c.ClientEnd.Update();
            }
            if (c.SupplierEnd.Navigable != direction.TargetNavigateability.Name)
            {
                c.SupplierEnd.Navigable = direction.TargetNavigateability.Name;
                c.SupplierEnd.Update();
            }
        }

        public static Option<EA.Element> Instanciate(this ElementStereotype classifierStereotype, EA.Element classifier, EA.Package package)
        {
            return classifierStereotype.InstanceType.Select(instanceStereotype =>
            {
                var e = instanceStereotype.Create(package, "");
                e.ClassifierID = classifier.ElementID;
                e.Update();

                return e;
            });
        }
    }
}
