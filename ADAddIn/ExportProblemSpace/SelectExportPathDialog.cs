using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdAddIn.ExportProblemSpace
{
    public class SelectExportPathDialog
    {
        public void WithSelectedFile(Action<Stream> act)
        {
            var dialog = new SaveFileDialog();

            dialog.Filter = "XML files (*.xml)|*.xml";

            var res = dialog.ShowDialog();

            if (res == DialogResult.OK)
            {
                using (var stream = dialog.OpenFile())
                {
                    act(stream);
                }
            }
        }
    }
}
