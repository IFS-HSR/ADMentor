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
    public partial class TailorPropertyTreeForm : Form
    {
        private PropertyTree propertyTree;

        public TailorPropertyTreeForm()
        {
            InitializeComponent();

            filterTreeView.CheckBoxes = true;
            filterTreeView.AfterCheck += filterTreeView_AfterCheck;

            hierarchyTreeView.CheckBoxes = true;
            hierarchyTreeView.AfterCheck += hierarchyTreeView_AfterCheck;
        }

        public Option<PropertyTree> Tailor(PropertyTree tree)
        {
            propertyTree = tree;

            UpdateFilter();

            var result = ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var hierarchyRoot = hierarchyTreeView.Nodes[0];
                return Options.Some(propertyTree);
            }
            else
            {
                return Options.None<PropertyTree>();
            }
        }

        private void UpdateFilter()
        {
            filterTreeView.Nodes.Clear();
            propertyTree.Properties.ForEach(prop =>
            {
                var node = new TreeNode(prop.Key, prop.Select(val => new TreeNode(val)).ToArray());
                filterTreeView.Nodes.Add(node);
            });
        }

        private void filterTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // only execute if event has been fired due to user interaction
            if (e.Action != TreeViewAction.Unknown)
            {
                if (e.Node.Checked)
                    SelectParent(e.Node);
                else
                    DeselectChildren(e.Node);
            }
        }

        private void hierarchyTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // only execute if event has been fired due to user interaction
            if (e.Action != TreeViewAction.Unknown)
            {
                if (e.Node.Checked)
                    SelectParent(e.Node);
                else
                    DeselectChildren(e.Node);
            }
        }

        private void CheckAll(TreeNodeCollection nodes)
        {
            nodes.Cast<TreeNode>().ForEach(n =>
            {
                n.Checked = true;
                CheckAll(n.Nodes);
            });
        }

        private void DeselectChildren(TreeNode node)
        {
            node.Nodes.Cast<TreeNode>().ForEach(child =>
            {
                child.Checked = false;
                DeselectChildren(child);
            });
        }

        private void SelectParent(TreeNode node)
        {
            node.Parent.AsOption().Do(parent =>
            {
                parent.Checked = true;
                SelectParent(parent);
            });
        }
    }
}
