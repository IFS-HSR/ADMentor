using AdAddIn.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace AdAddIn.ExportProblemSpace
{
    public partial class CreateFilterForm : Form
    {
        public CreateFilterForm()
        {
            InitializeComponent();
        }

        public Func<IEnumerable<Field<String>>> GetFields { get; set; }

        public Func<Field<String>, IEnumerable<Operator>> GetOperators { get; set; }

        public Func<Field<String>, Operator, IEnumerable<String>> GetProposedValues { get; set; }

        internal Option<ModelFilter> SelectFilter()
        {
            UpdateFields();

            var res = ShowDialog();

            if (res == System.Windows.Forms.DialogResult.OK)
            {
                return Options.Some(new ModelFilter.BinaryFilter(
                    fieldBox.SelectedItem as Field<String>,
                    operatorBox.SelectedItem as Operator,
                    valueBox.Text));
            }
            else
            {
                return Options.None<ModelFilter>();
            }
        }

        private void UpdateFields()
        {
            fieldBox.Items.Clear();

            var fields = GetFields();
            fieldBox.Items.AddRange(fields.ToArray());

            fields.FirstOption().Do(firstField =>
            {
                fieldBox.SelectedItem = firstField;
            });

            UpdateOperators();
        }

        private void UpdateOperators()
        {
            operatorBox.Items.Clear();

            fieldBox.SelectedItem.Match<Field<String>>()
                .Do(selectedField =>
                {
                    var ops = GetOperators(selectedField);
                    operatorBox.Items.AddRange(ops.ToArray());

                    ops.FirstOption().Do(firstOp =>
                    {
                        operatorBox.SelectedItem = firstOp;
                    });
                });

            UpdateValues();
        }

        private void UpdateValues()
        {
            valueBox.Items.Clear();

            fieldBox.SelectedItem.Match<Field<String>>()
                .Zip(operatorBox.SelectedItem.Match<Operator>())
                .ForEach((selectedField, selectedOp) =>
                {
                    var proposedVals = GetProposedValues(selectedField, selectedOp);
                    valueBox.Items.AddRange(proposedVals.ToArray());
                });
        }

        private void fieldBox_SelectedIndexChanged(object _, EventArgs __)
        {
            UpdateOperators();
        }

        private void operatorBox_SelectedIndexChanged(object _, EventArgs __)
        {
            UpdateValues();
        }
    }
}
