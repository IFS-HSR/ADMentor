using ADMentor.ADTechnology;
using ADMentor.DataAccess;
using EAAddInBase;
using EAAddInBase.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace ADMentor.Validation
{
    sealed class ValidateConflictingOptionsCommand : ICommand<ModelEntity.Connector, Option<ValidationMessage>>
    {
        private readonly AdRepository Repo;

        public ValidateConflictingOptionsCommand(AdRepository repo)
        {
            Repo = repo;
        }

        public Option<ValidationMessage> Execute(ModelEntity.Connector c)
        {
            var conflictingEnds = from source in c.Source(Repo.GetElement)
                                  from sourceOpt in source.TryCast<OptionOccurrence>()
                                  where sourceOpt.State == SolutionSpace.OptionState.Chosen
                                  from target in c.Target(Repo.GetElement)
                                  from targetOpt in target.TryCast<OptionOccurrence>()
                                  where targetOpt.State == SolutionSpace.OptionState.Chosen
                                  select Tuple.Create(sourceOpt, targetOpt);

            return (c.Is(ConnectorStereotypes.ConflictsWith) && (conflictingEnds).Any()).Then(
                () => ValidationMessage.Error("Conflicting options chosen"));
        }

        public bool CanExecute(ModelEntity.Connector _)
        {
            return true;
        }
    }
}
