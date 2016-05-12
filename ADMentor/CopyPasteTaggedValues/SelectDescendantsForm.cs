using EAAddInBase.DataAccess;
using EAAddInBase.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADMentor.CopyPasteTaggedValues
{
    public partial class SelectDescendantsForm : Form
    {
        public SelectDescendantsForm()
        {
            InitializeComponent();
        }

        public Option<IEnumerable<ModelEntity>> Select(PackageTree entities)
        {
            entityTree.Nodes.Clear();
            entityTree.Nodes.Add(CreateNode(entities));

            var res = ShowDialog();

            if (res == System.Windows.Forms.DialogResult.OK)
            {
                return Options.Some(SelectedEntities(entityTree.Nodes.Cast<TreeNode>()));
            }
            else
            {
                return Options.None<IEnumerable<ModelEntity>>();
            }
        }

        private IEnumerable<ModelEntity> SelectedEntities(IEnumerable<TreeNode> nodes)
        {
            return nodes.SelectMany(n =>
            {
                var currentTree = (n.Tag as PackageTree);
                var current = n.Checked && currentTree.Selectable
                    ? new[] { currentTree.Entity }
                    : new ModelEntity[] { };

                return current.Concat(SelectedEntities(n.Nodes.Cast<TreeNode>()));
            });
        }

        private TreeNode CreateNode(PackageTree entityTree)
        {
            var node = new TreeNode(
                entityTree.Entity.ToString(),
                entityTree.Children.Select(CreateNode).ToArray());

            if (entityTree.Selectable)
            {
                node.ImageIndex = 0;
                node.SelectedImageIndex = 0;
            }
            else
            {
                node.ForeColor = SystemColors.GrayText;
                node.ImageIndex = 2;
                node.SelectedImageIndex = 2;
            }
            node.Tag = entityTree;
            node.Expand();

            return node;
        }

        private void ToggleNodeChecked(TreeNode node)
        {
            if ((node.Tag as PackageTree).Selectable)
            {
                node.Checked = !node.Checked;
                if (node.Checked)
                {
                    node.ImageIndex = 1;
                    node.SelectedImageIndex = 1;
                }
                else
                {
                    node.ImageIndex = 0;
                    node.SelectedImageIndex = 0;
                }
            }
        }

        private void entityTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            ToggleNodeChecked(e.Node);
        }

        private void entityTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            ToggleNodeChecked(e.Node);
        }

        private void entityTree_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                ToggleNodeChecked(entityTree.SelectedNode);
            }
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            entityTree.Nodes.Cast<TreeNode>().ForEach(SelectAll);
        }

        private void buttonDeselectAll_Click(object sender, EventArgs e)
        {
            entityTree.Nodes.Cast<TreeNode>().ForEach(DeselectAll);
        }

        private void SelectAll(TreeNode node)
        {
            if (!node.Checked)
            {
                ToggleNodeChecked(node);
            }
            node.Nodes.Cast<TreeNode>().ForEach(SelectAll);
        }

        private void DeselectAll(TreeNode node)
        {
            if (node.Checked)
            {
                ToggleNodeChecked(node);
            }
            node.Nodes.Cast<TreeNode>().ForEach(DeselectAll);
        }
    }
}
