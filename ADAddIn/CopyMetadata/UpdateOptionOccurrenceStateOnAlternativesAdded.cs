using AdAddIn.DataAccess;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.CopyMetadata
{
    public class UpdateProblemOccurrenceStateOnAlternativesAdded : ICommand<ModelEntity.Connector, EntityModified>
    {
        private readonly ModelEntityRepository Repo;

        public UpdateProblemOccurrenceStateOnAlternativesAdded(ModelEntityRepository repo)
        {
            Repo = repo;
        }

        public EntityModified Execute(ModelEntity.Connector c)
        {
            (from source in c.Source(Repo.GetElement)
             from problemOcc in source.TryCast<ProblemOccurrence>()
             select problemOcc)
                .Do(problemOccurrence =>
                 {
                     problemOccurrence.State = problemOccurrence.DeduceState(problemOccurrence.GetAlternatives(Repo.GetElement));
                     Repo.PropagateChanges(problemOccurrence);
                 });

            return EntityModified.NotModified;
        }

        public bool CanExecute(ModelEntity.Connector _)
        {
            return true;
        }
    }
}
