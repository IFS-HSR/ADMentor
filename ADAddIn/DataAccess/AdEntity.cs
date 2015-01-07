using AdAddIn.ADTechnology;
using EAAddInFramework.DataAccess;
using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
                // name must be copied to instance or it wont have a name in package browser
                EaObject.Name = classifier.EaObject.Name;
                EaObject.Notes = classifier.EaObject.Notes;
                EaObject.Tag = classifier.EaObject.Tag; // keywords
                EaObject.Alias = classifier.EaObject.Alias;
                EaObject.Status = classifier.EaObject.Status;
                EaObject.Complexity = classifier.EaObject.Complexity;
                EaObject.Version = classifier.EaObject.Version;
                EaObject.Phase = classifier.EaObject.Phase;
                EaObject.Author = classifier.EaObject.Author;

                ProblemSpace.Problem.TaggedValues
                    .Concat(ProblemSpace.Option.TaggedValues)
                    .Distinct()
                    .SelectMany(tv => classifier.Get(tv).Select(v => Tuple.Create(tv, v)))
                    .ForEach((taggedValue, valueInClassifier) =>
                    {
                        this.Set(taggedValue, valueInClassifier);
                    });

                EaObject.Update();
            });
        }
    }
}
