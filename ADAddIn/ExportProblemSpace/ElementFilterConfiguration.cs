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
    public partial class ElementFilterConfiguration : Form
    {
        public ElementFilterConfiguration(Func<Option<ModelFilter>> createNewFilter)
        {
            InitializeComponent();
            filterTreeView.CreateNewFilter = createNewFilter;
        }

        internal void Display()
        {
            var result = ShowDialog();
        }
    }
}
