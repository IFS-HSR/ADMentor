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

namespace AdAddIn.InstantiateProblemSpace
{
    public partial class InstantiateSolutionForm : Form
    {
        public InstantiateSolutionForm()
        {
            InitializeComponent();
        }

        public Option<String> GetSolutionName()
        {
            textBoxName.Clear();

            if (ShowDialog() == System.Windows.Forms.DialogResult.OK && !textBoxName.Text.Equals(""))
            {
                return Options.Some(textBoxName.Text);
            }
            else
            {
                return Options.None<String>();
            }
        }
    }
}
