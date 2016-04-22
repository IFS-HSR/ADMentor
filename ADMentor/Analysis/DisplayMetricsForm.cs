using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EAAddInBase.Utils;

namespace ADMentor.Analysis
{
    public partial class DisplayMetricsForm : Form
    {
        private IEnumerable<MetricEntry> data;

        public DisplayMetricsForm()
        {
            InitializeComponent();
        }

        public void Display(IEnumerable<MetricEntry> data)
        {
            this.data = data;

            var categories = (from e in data
                              group e by e.Category into g
                              select g.Key).ToArray();

            categoryListBox.Items.AddRange(categories);

            if (categories.Length > 0)
            {
                categoryListBox.SetItemChecked(0, true);
            }

            UpdateDataGrid();

            ShowDialog();
        }

        private void UpdateDataGrid()
        {
            dataGridView.DataSource =
                (from e in data
                 where categoryListBox.CheckedItems.Contains(e.Category)
                 select e).ToList();
        }

        private void categoryListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (this.Visible)
            {
                // Delay execution until CheckedItems have been updated
                BeginInvoke((MethodInvoker)(() => UpdateDataGrid()));
            }
        }
    }
}
