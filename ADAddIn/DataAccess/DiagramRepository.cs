using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.DataAccess
{
    public class DiagramRepository
    {
        private readonly IReadableAtom<EA.Repository> Repo;

        public DiagramRepository(IReadableAtom<EA.Repository> repo)
        {
            Repo = repo;
        }

        public Option<EA.Diagram> GetCurrentDiagram()
        {
            return Repo.Val.GetCurrentDiagram().AsOption();
        }

        public void ReloadDiagram(EA.Diagram d)
        {
            Repo.Val.ReloadDiagram(d.DiagramID);
        }

        public void SaveDiagram(EA.Diagram d)
        {
            Repo.Val.SaveDiagram(d.DiagramID);
        }

        public Boolean Contains(EA.Diagram d, EA.Element e)
        {
            return d.DiagramObjects.Cast<EA.DiagramObject>().Any(obj => obj.ElementID == e.ElementID);
        }

        public Option<EA.DiagramObject> FindDiagramObject(EA.Diagram d, EA.Element e)
        {
            return d.DiagramObjects.Cast<EA.DiagramObject>().FirstOption(obj => obj.ElementID == e.ElementID);
        }

        public EA.DiagramObject AddToDiagram(EA.Diagram d, EA.Element e, int left = 0, int right = 0, int top = 0, int bottom = 0)
        {
            var pos = String.Format("l={0};r={1};t={2};b={3};", left, right, top, bottom);
            var obj = d.DiagramObjects.AddNew(pos, "") as EA.DiagramObject;
            obj.ElementID = e.ElementID;
            obj.Update();
            d.DiagramObjects.Refresh();

            return obj;
        }

        public EA.Diagram Create(EAAddInFramework.MDGBuilder.Diagram diagramType, EA.Package package, string name)
        {
            var diagram = package.Diagrams.AddNew(name, diagramType.Type.Name) as EA.Diagram;
            diagram.StyleEx = String.Format("MDGDgm={0}::{1};", ADTechnology.Technologies.AD.ID, diagramType.Name);
            diagram.Update();
            package.Diagrams.Refresh();
            return diagram;
        }
    }
}
