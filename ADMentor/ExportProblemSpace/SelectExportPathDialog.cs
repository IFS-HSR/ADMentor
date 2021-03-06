﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADMentor.ExportProblemSpace
{
    public class SelectExportPathDialog
    {
        public void WithSelectedFile(String fileNamePrefix, Action<Stream> act)
        {
            var dialog = new SaveFileDialog();

            dialog.Filter = "XML files (*.xml)|*.xml";
            dialog.FileName = fileNamePrefix + ".xml";

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
