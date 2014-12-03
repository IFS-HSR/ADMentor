using EAAddInFramework.DataAccess;
using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace EAAddInFramework
{
    public class ValidationRule
    {
        public ValidationRule(String category, ICommand<ModelEntity, Option<ValidationMessage>> cmd)
        {
            Category = category;
            Command = cmd;
        }

        public string Category { get; private set; }

        public ICommand<ModelEntity, Option<ValidationMessage>> Command { get; private set; }
    }

    public class ValidationMessage
    {
        public static ValidationMessage Error(String msg)
        {
            return new ValidationMessage(msg, EA.EnumMVErrorType.mvError);
        }

        public static ValidationMessage Warning(String msg)
        {
            return new ValidationMessage(msg, EA.EnumMVErrorType.mvWarning);
        }

        public static ValidationMessage Information(String msg)
        {
            return new ValidationMessage(msg, EA.EnumMVErrorType.mvInformation);
        }

        private ValidationMessage(String msg, EA.EnumMVErrorType errorLevel)
        {
            Message = msg;
            ErrorLevel = errorLevel;
        }

        public string Message { get; private set; }

        public EA.EnumMVErrorType ErrorLevel { get; private set; }
    }

    class ValidationHandler
    {
        private readonly IReadableAtom<EA.Repository> Repository;

        private readonly ImmutableDictionary<string, ValidationRule> Rules;

        private readonly ImmutableDictionary<string, string> CategoryIds;

        public ValidationHandler(IReadableAtom<EA.Repository> repository, ImmutableDictionary<String, String> categoryIds, ImmutableDictionary<String, ValidationRule> rules)
        {
            Repository = repository;
            CategoryIds = categoryIds;
            Rules = rules;
        }

        public void ExecuteRule(string ruleId, Func<ModelEntity> getEntity)
        {
            Rules.Get(ruleId).Do(rule =>
            {
                var entity = getEntity();
                if (rule.Command.CanExecute(entity))
                {
                    rule.Command.Execute(entity).Do(msg =>
                    {
                        var cid = CategoryIds.Get(rule.Category).Value;
                        Repository.Val.GetProjectInterface().PublishResult(ruleId, msg.ErrorLevel, msg.Message);
                    });
                }
            });
        }
    }
}
