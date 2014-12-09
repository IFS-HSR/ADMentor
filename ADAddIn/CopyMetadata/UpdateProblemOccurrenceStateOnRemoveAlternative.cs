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
    public class UpdateProblemOccurrenceStateOnRemoveAlternative : ICommand<OptionOccurrence, DeleteEntity>
    {
        private readonly ModelEntityRepository Repo;

        public UpdateProblemOccurrenceStateOnRemoveAlternative(ModelEntityRepository repo)
        {
            Repo = repo;
        }

        public DeleteEntity Execute(OptionOccurrence alternative)
        {
            alternative.GetAssociatedProblemOccurrences(Repo.GetElement).ForEach(problemOccurrence =>
            {
                var alternatives = from alt in problemOccurrence.GetAlternatives(Repo.GetElement)
                                   where !alt.Equals(alternative)
                                   select alt;

                problemOccurrence.State = problemOccurrence.DeduceState(alternatives);

                Repo.PropagateChanges(problemOccurrence);
            });

            return DeleteEntity.Delete;
        }

        public bool CanExecute(OptionOccurrence _)
        {
            return true;
        }

        public ICommand<ModelEntity, DeleteEntity> AsOnDeleteEntityHandler()
        {
            return this.Adapt((ModelEntity entity) => {
                return entity.TryCast<OptionOccurrence>().OrElse(() =>
                {
                    return from connector in entity.TryCast<ModelEntity.Connector>()
                           from target in connector.Target(Repo.GetElement)
                           from oo in target.TryCast<OptionOccurrence>()
                           select oo;
                });
            });
        }
    }
}
