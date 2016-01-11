using EAAddInBase.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADMentor.ADRepoConnector
{
    public partial class CommitForm : Form
    {
        public CommitForm()
        {
            InitializeComponent();
        }

        internal Option<Commit> AskForCommit(Model model, IEnumerable<Tag> tags)
        {
            modelName.Text = model.root.Join("/");
            message.Text = "";
            predecessors.Items.Clear();

            tags.OrderBy(t => t.ToString()).ForEach(tag =>
            {
                predecessors.Items.Add(tag, tag.ToString() == model.root.Join("/"));
            });

            var result = ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var commit = new Commit
                {
                    message = message.Text,
                    model = model,
                    prev = predecessors.CheckedItems.Cast<Tag>().Select(t => t.commitId)
                };

                return Options.Some(commit);
            }
            else
            {
                return Options.None<Commit>();
            }
        }
    }
}
