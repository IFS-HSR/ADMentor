using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdAddIn.ExportProblemSpace
{
    public partial class ElementFilterConfiguration : Form
    {
        public ElementFilterConfiguration()
        {
            InitializeComponent();
        }

        internal void Display()
        {
            var result = ShowDialog();
        }
    }
}
