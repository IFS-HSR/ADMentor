using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace EAAddInFramework.DataAccess
{
    public interface IProjectVisitor
    {
        bool Visit(EA.Repository repository);
        bool Visit(EA.Package package);
        void Visit(EA.Diagram diagram);
        void Visit(EA.Element element);
    }

    public static class AcceptProjectVisitorExtensions
    {
        public static void Accept(this EA.Repository repo, IProjectVisitor v)
        {
            if (v.Visit(repo))
            {
                repo.Models.Cast<EA.Package>().ForEach(p => p.Accept(v));
            }
        }

        public static void Accept(this EA.Package package, IProjectVisitor v)
        {
            if (v.Visit(package))
            {
                package.Elements.Cast<EA.Element>().ForEach(e => e.Accept(v));
                package.Packages.Cast<EA.Package>().ForEach(p => p.Accept(v));
            }
        }

        public static void Accept(this EA.Diagram diagram, IProjectVisitor v)
        {
            v.Visit(diagram);
        }

        public static void Accept(this EA.Element element, IProjectVisitor v)
        {
            v.Visit(element);
        }
    }
}
