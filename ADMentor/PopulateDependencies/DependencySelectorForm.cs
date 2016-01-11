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
using EAAddInBase;
using ADMentor.DataAccess;
using EAAddInBase.DataAccess;

namespace ADMentor.PopulateDependencies
{
    public partial class DependencySelectorForm : Form, IDependencySelector
    {
        private readonly ModelEntityRepository Repo;

        public DependencySelectorForm(ModelEntityRepository repo)
        {
            Repo = repo;

            InitializeComponent();

            dependencyTreeView.CheckBoxes = true;
            dependencyTreeView.AfterCheck += dependencyTreeView_AfterCheck;
        }

        public Option<LabeledTree<ElementInstantiation, ModelEntity.Connector>> GetSelectedDependencies(
            LabeledTree<ElementInstantiation, ModelEntity.Connector> availableDependencies)
        {
            dependencyTreeView.Nodes.Clear();
            dependencyTreeView.Nodes.Add(ToTreeNode(availableDependencies));
            dependencyTreeView.Nodes.Cast<TreeNode>().ForEach(tn => tn.Expand());

            var result = ShowDialog();

            var markedTree = (result == System.Windows.Forms.DialogResult.OK)
                .Then(() => MarkSelectedNodes(availableDependencies, dependencyTreeView.Nodes[0]));

            dependencyTreeView.Nodes.Clear();
            return markedTree;
        }

        private LabeledTree<ElementInstantiation, ModelEntity.Connector> MarkSelectedNodes(LabeledTree<ElementInstantiation, ModelEntity.Connector> problemSpace, TreeNode treeNode)
        {
            var edges = from pair in problemSpace.Edges.Zip(treeNode.Nodes.Cast<TreeNode>())
                        select LabeledTree.Edge(pair.Item1.Label, MarkSelectedNodes(pair.Item1.Target, pair.Item2));

            return (!problemSpace.Label.Instance.IsDefined && treeNode.Checked)
                .Then(() => LabeledTree.Node(new ElementInstantiation(problemSpace.Label.Element, selected: true), edges))
                .Else(() => LabeledTree.Node(problemSpace.Label, edges));
        }

        private TreeNode ToTreeNode(LabeledTree<ElementInstantiation, ModelEntity.Connector> dependencyNode)
        {
            var node = new TreeNode(dependencyNode.Label.Element.Name, ToTreeNodes(dependencyNode.Edges));
            if (dependencyNode.Label.Instance.IsDefined)
                node.ForeColor = System.Drawing.SystemColors.GrayText;
            return node;
        }

        private TreeNode[] ToTreeNodes(IEnumerable<LabeledTree<ElementInstantiation, ModelEntity.Connector>.Edge> edges)
        {
            return edges.Select(edge =>
            {
                var stype = edge.Label.GetStereotype(ADTechnology.Technologies.AD.ConnectorStereotypes).Value;
                var connectorName = edge.Label.EaObject.SupplierID == edge.Target.Label.Element.Id
                    ? stype.DisplayName
                    : stype.ReverseDisplayName.GetOrElse(stype.DisplayName);
                var label = String.Format("{0}: {1}", connectorName, edge.Target.Label.Element.Name);
                var node = new TreeNode(label, ToTreeNodes(edge.Target.Edges));
                if (edge.Target.Label.Instance.IsDefined)
                    node.ForeColor = System.Drawing.SystemColors.GrayText;
                return node;
            }).ToArray();
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            UpdateAll(dependencyTreeView.Nodes, true);
        }

        private void buttonDeselectAll_Click(object sender, EventArgs e)
        {
            UpdateAll(dependencyTreeView.Nodes, false);
        }

        private void UpdateAll(TreeNodeCollection treeNodeCollection, bool check)
        {
            treeNodeCollection.Cast<TreeNode>().ForEach(node =>
            {
                node.Checked = check;
                UpdateAll(node.Nodes, check);
            });
        }

        private void dependencyTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Checked)
            {
                e.Node.Parent.AsOption().Do(parent =>
                {
                    parent.Checked = true;
                });
            }
            else
            {
                e.Node.Nodes.Cast<TreeNode>().ForEach(child =>
                {
                    child.Checked = false;
                });
            }
        }
    }
}
