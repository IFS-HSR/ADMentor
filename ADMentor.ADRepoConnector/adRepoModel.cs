using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;
using System.ComponentModel;

namespace ADMentor.ADRepoConnector
{
    interface Tag
    {
        IEnumerable<String> modelRoot { get; }
        String commitId { get; }
    }

    sealed class HeadTag : Tag
    {
        public IEnumerable<String> modelRoot { get; set; }
        public String commitId { get; set; }
        public override String ToString()
        {
            return modelRoot.Join("/");
        }
    }

    sealed class CustomTag : Tag
    {
        public String name { get; set; }
        public IEnumerable<String> modelRoot { get; set; }
        public String commitId { get; set; }
        public override String ToString()
        {
            return modelRoot.Join("/") + "[" + name + "]";
        }
    }

    class Commit
    {
        public String message { get; set; }

        public Model model { get; set; }

        public IEnumerable<String> prev { get; set; }
    }

    class Model
    {
        public IEnumerable<String> root { get; set; }
        public IEnumerable<Element> elements { get; set; }
        public IEnumerable<Relation> relations { get; set; }
        public IEnumerable<Diagram> diagrams { get; set; }
    }

    class Element
    {
        public String tpe { get; set; }
        public IEnumerable<String> path { get; set; }
        public IDictionary<String, String> attributes { get; set; }
    }

    class Relation
    {
        public String tpe { get; set; }
        public IEnumerable<String> from { get; set; }
        public IEnumerable<String> to { get; set; }
    }

    class Diagram
    {
        public IEnumerable<String> dataElement { get; set; }

        public IEnumerable<DiagramObject> objects { get; set; }
    }

    class DiagramObject
    {
        public IEnumerable<String> element { get; set; }

        public int x { get; set; }

        public int y { get; set; }
    }
}
