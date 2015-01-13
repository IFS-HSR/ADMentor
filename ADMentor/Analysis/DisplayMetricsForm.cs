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
        public DisplayMetricsForm()
        {
            InitializeComponent();
        }

        public void Display(Metric data)
        {
            dataGridView.DataSource = Flatten(data).ToList();

            ShowDialog();
        }

        private IEnumerable<Tuple<String, String>> Flatten(Metric m, String prefix = "")
        {
            return (new[] { ToRow(m, prefix) })
                .Concat(from cat in m.TryCast<Category>()
                        from member in cat.Members
                        from flatened in Flatten(member, prefix + "- ")
                        select flatened);
        }

        private Tuple<String, String> ToRow(Metric m, String prefix)
        {
            return m.Match<Metric, Tuple<String, String>>()
                .Case<Category>(c => Tuple.Create(prefix + c.Name, ""))
                .Case<Entry>(e => Tuple.Create(prefix + e.Key, e.Value))
                .GetOrThrowNotImplemented();
        }
    }
}
