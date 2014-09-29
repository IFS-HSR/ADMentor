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
                EaObject.Tag = classifier.EaObject.Tag; // keywords
                EaObject.Alias = classifier.EaObject.Alias;
                EaObject.Status = classifier.EaObject.Status;
                EaObject.Complexity = classifier.EaObject.Complexity;
                EaObject.Version = classifier.EaObject.Version;
                EaObject.Phase = classifier.EaObject.Phase;
                EaObject.Author = classifier.EaObject.Author;

                EaObject.Update();
            });
        }
    }
}
