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
    public abstract class ValidationRule
    {
        public ValidationRule(String category)
        {
            Category = category;
        }

        public string Category { get; private set; }

        public virtual void Prepare() { }

        public abstract Option<ValidationMessage> Execute(ModelEntity e);

        public virtual void CleanUp() { }

        public static ValidationRule FromCommand<E>(String category, ICommand<E, Option<ValidationMessage>> cmd)
            where E : ModelEntity
        {
            return new ValidationRuleAdapter<E>(category, cmd);
        }
    }

    public class ValidationRuleAdapter<E> : ValidationRule where E : ModelEntity
    {
        private readonly ICommand<E, Option<ValidationMessage>> Cmd;

        public ValidationRuleAdapter(String category, ICommand<E, Option<ValidationMessage>> cmd)
            : base(category)
        {
            Cmd = cmd;
        }

        public override Option<ValidationMessage> Execute(ModelEntity entity)
        {
            return from e in entity.TryCast<E>()
                   where Cmd.CanExecute(e)
                   from res in Cmd.Execute(e)
                   select res;
        }
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
        private readonly IEnumerable<ValidationRule> Rules;
        private Dictionary<String, ValidationRule> IdToRule = new Dictionary<string, ValidationRule>();
        private Dictionary<String, String> IdToCategory = new Dictionary<string, string>();

        public ValidationHandler(IReadableAtom<EA.Repository> repository, IEnumerable<ValidationRule> rules)
        {
            Repository = repository;
            Rules = rules;
        }

        public void RegisterRules()
        {
            var projectInterface = Repository.Val.GetProjectInterface();
            var categories = (from rule in Rules
                              select rule.Category).Distinct();

            var catToId = new Dictionary<String, String>();

            categories.ForEach(cat =>
            {
                var id = projectInterface.DefineRuleCategory(cat);
                catToId[cat] = id;
                IdToCategory[id] = cat;
            });

            Rules.ForEach(rule =>
            {
                var id = projectInterface.DefineRule(catToId[rule.Category], EA.EnumMVErrorType.mvError, "");
                IdToRule[id] = rule;
            });
        }

        public void PrepareRules(String[] selectedCategories)
        {
            var selectedAddInCategories = from cat in selectedCategories
                                          from addInCat in IdToCategory.Get(cat)
                                          select addInCat;

            (from rule in Rules
             where selectedAddInCategories.Contains(rule.Category)
             select rule).ForEach(rule =>
             {
                 rule.Prepare();
             });
        }

        public void ExecuteRule(String ruleId, Func<ModelEntity> getEntity)
        {
            IdToRule.Get(ruleId).Do(rule =>
            {
                var entity = getEntity();
                rule.Execute(entity).Do(msg =>
                {
                    Repository.Val.GetProjectInterface().PublishResult(ruleId, msg.ErrorLevel, msg.Message);
                });
            });
        }

        public void CleanUpRules(String[] selectedCategories)
        {
            var selectedAddInCategories = from cat in selectedCategories
                                          from addInCat in IdToCategory.Get(cat)
                                          select addInCat;

            (from rule in Rules
             where selectedAddInCategories.Contains(rule.Category)
             select rule).ForEach(rule =>
             {
                 rule.CleanUp();
             });
        }
    }
}
