using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EAAddInBase.Utils;

namespace ADMentor.CopyPasteTaggedValues
{
    public partial class SelectTaggedValuesForm : Form
    {
        public SelectTaggedValuesForm()
        {
            InitializeComponent();
        }

        public Option<IImmutableDictionary<String, String>> GetSelected(IImmutableDictionary<String, String> taggedValues)
        {
            taggeValuesList.Items.Clear();
            taggedValues.ForEach(tv =>
            {
                taggeValuesList.Items.Add(tv, true);
            });

            var result = ShowDialog();

            return (result == System.Windows.Forms.DialogResult.OK)
                .Then(() => ImmutableDictionary.CreateRange(taggeValuesList.CheckedItems.Cast<KeyValuePair<String, String>>()));
        }
    }
}
