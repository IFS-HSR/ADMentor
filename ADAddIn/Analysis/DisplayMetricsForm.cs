using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdAddIn.Analysis
{
    public partial class DisplayMetricsForm : Form
    {
        public DisplayMetricsForm()
        {
            InitializeComponent();
        }

        public void Display(Metric data)
        {
            metricsTextBox.Text = data.ToString();

            ShowDialog();
        }
    }
}
