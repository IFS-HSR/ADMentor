using AdAddIn.ADTechnology;
using AdAddIn.DataAccess;
using EAAddInFramework;
using EAAddInFramework.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace AdAddIn.ExportProblemSpace
{
    class TestFilterConfigurationCommand : ICommand<ModelEntity.Package, Unit>
    {
        public Unit Execute(ModelEntity.Package p)
        {
            var form = new ElementFilterConfiguration(
                () => SelectNewFilter(p));

            form.Display();

            return Unit.Instance;
        }

        private Option<ModelFilter> SelectNewFilter(ModelEntity.Package p)
        {
            var createFilterForm = new CreateFilterForm();

            createFilterForm.GetFields = GetFields;
            createFilterForm.GetOperators = GetOperators;
            createFilterForm.GetProposedValues = GetProposedValues;

            return createFilterForm.SelectFilter();
        }

        private IEnumerable<Field<String>> GetFields()
        {
            return new Field<String>[] {
                new Field<String>.Type(),
                new Field<String>.ElementName(),
                new Field<String>.TaggedValueField(SolutionSpace.OptionStateTag),
                new Field<String>.TaggedValueField(SolutionSpace.ProblemOccurrenceStateTag)
            };
        }

        private IEnumerable<Operator> GetOperators(Field<String> selectedField)
        {
            return new Operator[] {
                new Operator.Is(),
                new Operator.Matches()
            };
        }

        private IEnumerable<string> GetProposedValues(Field<string> selectedField, Operator selectedOperator)
        {
            return selectedField.TryCast<Field<String>.Type>()
                .Match(_ => new[] { "Element", "Package", "Diagram" }, () => Enumerable.Empty<String>());
        }

        public bool CanExecute(ModelEntity.Package p)
        {
            return true;
        }

        public ICommand<Option<ModelEntity>, object> AsMenuCommand()
        {
            return this.Adapt((Option<ModelEntity> contextItem) =>
            {
                return from ci in contextItem
                       from package in ci.TryCast<ModelEntity.Package>()
                       select package;
            });
        }
    }
}
