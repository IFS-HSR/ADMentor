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

namespace ADMentor.CopyMetadata
{
    /// <summary>
    /// Copies some data from the classifier if an Option/Problem Occurrence has been created from
    /// an existing Option/Problem.
    /// </summary>
    sealed class UpdateMetadataOfNewElementsCommand : ICommand<AdEntity, EntityModified>
    {
        private readonly ModelEntityRepository Repo;

        public UpdateMetadataOfNewElementsCommand(ModelEntityRepository repo)
        {
            Repo = repo;
        }

        public EntityModified Execute(AdEntity entity)
        {
            entity.CopyDataFromClassifier(Repo.GetElement);
            return EntityModified.Modified;
        }

        public bool CanExecute(AdEntity _)
        {
            return true;
        }
    }
}
