using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using AdAddIn.PopulateDependencies;
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
    public class UpdateProblemOccurrenceStateCommand : ICommand<OptionOccurrence, Unit>
    {
        private readonly ModelEntityRepository Repo;

        public UpdateProblemOccurrenceStateCommand(ModelEntityRepository repo)
        {
            Repo = repo;
        }

        public Unit Execute(OptionOccurrence optionOcc)
        {
            var problemOccs = from problemOcc in optionOcc.GetAssociatedProblemOccurrences(Repo.GetElement)
                              select problemOcc;

            problemOccs.ForEach(problemOcc =>
            {
                var alternatives = problemOcc.GetAlternatives(Repo.GetElement);
                problemOcc.State = problemOcc.DeduceState(alternatives);
                Repo.PropagateChanges(problemOcc);
            });

            return Unit.Instance;
        }

        public bool CanExecute(OptionOccurrence _)
        {
            return true;
        }

        public ICommand<ModelEntity, object> AsEntityModifiedHandler()
        {
            return this.Adapt((ModelEntity entity) =>
            {
                return entity.Match<OptionOccurrence>();
            });
        }
    }
}
