using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInFramework.DataAccess
{
    public class Collector<T> : IProjectVisitor
    {
        private Func<T, EA.Package, T> collectPackage;
        private Func<T, EA.Diagram, T> collectDiagram;
        private Func<T, EA.Element, T> collectElement;
        public Collector()
        {
            this.collectPackage = (s, p) => s;
            this.collectDiagram = (s, p) => s;
            this.collectElement = (s, p) => s;
        }
        public bool Visit(EA.Repository repository)
        {
            return true;
        }
        public bool Visit(EA.Package package)
        {
            Result = collectPackage(Result, package);
            return true;
        }
        public void Visit(EA.Diagram diagram)
        {
            Result = collectDiagram(Result, diagram);
        }
        public void Visit(EA.Element element)
        {
            Result = collectElement(Result, element);
        }
        public T Result { get; private set; }
        public Collector<T> Init(T v)
        {
            Result = v;
            return this;
        }
        public Collector<T> CollectPackagesWith(Func<T, EA.Package, T> fn)
        {
            collectPackage = fn;
            return this;
        }
        public Collector<T> CollectElementsWith(Func<T, EA.Element, T> fn)
        {
            collectElement = fn;
            return this;
        }
    }

    public static class Collectors
    {
        public static Collector<T> Create<T>(T initValue)
        {
            return new Collector<T>()
                .Init(initValue);
        }
    }
}
