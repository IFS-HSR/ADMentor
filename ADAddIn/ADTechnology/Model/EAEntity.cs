using EAAddInFramework;
using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.ADTechnology.Model
{
    public class EAEntity<T> where T : EAEntity<T>
    {
        protected EAEntity(EA.Repository repo, EA.Element element)
        {
            Repo = repo;
            Element = element;
        }

        protected EA.Repository Repo { get; private set; }

        public EA.Element Element { get; private set; }

        public String Name { get { return Element.Name; } }

        public Option<EA.Element> Classifier
        {
            get
            {
                return Repo.TryGetElement(Element.ClassifierID);
            }
        }

        protected static Option<T> Create(ElementStereotype stereotype, EA.Element element, Func<T> createT)
        {
            if (element.Is(stereotype))
            {
                return Options.Some(createT());
            }
            else
            {
                return Options.None<T>();
            }
        }
    }
}
