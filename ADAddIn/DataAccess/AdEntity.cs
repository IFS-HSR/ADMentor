using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.DataAccess
{
    public abstract class AdEntity : ModelEntity.Element
    {
        public AdEntity(EA.Element e, IEntityWrapper wrapper) : base(e, wrapper) { }

        public void CopyDataFromClassifier(Func<int, Option<ModelEntity.Element>> getElementById)
        {
            getElementById(EaObject.ClassifierID).Do(classifier =>
            {
                EaObject.Name = classifier.EaObject.Name;
                EaObject.Notes = classifier.EaObject.Notes;
                EaObject.Update();
            });
        }
    }
}
