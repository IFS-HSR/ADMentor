using AdAddIn.DataAccess;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.Validation
{
    class MultipleProblemsAddressedByAnOptionCommand : ICommand<OptionEntity, Option<ValidationMessage>>
    {
        private readonly AdRepository Repo;

        public MultipleProblemsAddressedByAnOptionCommand(AdRepository repo)
        {
            Repo = repo;
        }

        public Option<ValidationMessage> Execute(OptionEntity option)
        {
            return (option.GetProblems(Repo.GetElement).Count() > 1)
                .Then(() => ValidationMessage.Warning("Multiple Problems addressed by an Option"));
        }

        public bool CanExecute(OptionEntity _)
        {
            return true;
        }
    }
}
