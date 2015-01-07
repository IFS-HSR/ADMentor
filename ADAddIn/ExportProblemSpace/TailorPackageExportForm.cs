using EAAddInFramework.DataAccess;
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
    public partial class TailorPackageExportForm : Form
    {
        private Func<IFilter<PropertyTree>, PropertyTree> GetModelHierarchy = null;
        private CompositeFilter<PropertyTree> OriginalFilter = null;
        private IFilter<PropertyTree> SelectedFilter = null;
        private PropertyTree ModelHierarchy = null;

        public TailorPackageExportForm()
        {
            InitializeComponent();

            filterTreeView.CheckBoxes = true;
            filterTreeView.AfterCheck += filterTreeView_AfterCheck;

            hierarchyTreeView.CheckBoxes = true;
            hierarchyTreeView.AfterCheck += hierarchyTreeView_AfterCheck;
        }

        public Option<PropertyTree> SelectFilter(CompositeFilter<PropertyTree> filter,
            Func<IFilter<PropertyTree>, PropertyTree> getModelHierarchy)
        {
            GetModelHierarchy = getModelHierarchy;
            OriginalFilter = filter;

            filterTreeView.Nodes.Clear();
            var filterNodes = ToTreeNodes(filter.Filters);
            filterTreeView.Nodes.AddRange(filterNodes.ToArray());

            UpdateHierarchyTreeView();

            var result = ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var hierarchyRoot = hierarchyTreeView.Nodes[0];
                return Options.Some(ToHierarchy(ModelHierarchy, hierarchyRoot.Nodes.Cast<TreeNode>()));
            }
            else
            {
                return Options.None<PropertyTree>();
            }
        }

        private TreeNode[] ToTreeNodes(IEnumerable<IFilter<PropertyTree>> filters)
        {
            return (from filter in filters
                    select filter.TryCast<CompositeFilter<PropertyTree>>().Fold(
                            composite => ToTreeNode(composite),
                            () => ToTreeNode(filter))).ToArray();
        }

        private TreeNode ToTreeNode(CompositeFilter<PropertyTree> filter)
        {
            var node = new TreeNode(filter.Name, ToTreeNodes(filter.Filters));
            node.Tag = filter;

            return node;
        }

        private TreeNode ToTreeNode(IFilter<PropertyTree> filter)
        {
            var node = new TreeNode(filter.Name);
            node.Tag = filter;

            return node;
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

        private void filterTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // only execute if event has been fired due to user interaction
            if (e.Action != TreeViewAction.Unknown)
            {
                if (e.Node.Checked)
                    SelectParent(e.Node);
                else
                    DeselectChildren(e.Node);

                UpdateHierarchyTreeView();
            }
        }

        private void UpdateSelectedFilter()
        {
            var filter = ToFilter(OriginalFilter, filterTreeView.Nodes.Cast<TreeNode>());

            SelectedFilter = filter;
        }

        private void UpdateModelHierarchy()
        {
            UpdateSelectedFilter();

            ModelHierarchy = GetModelHierarchy(SelectedFilter);
        }

        private void UpdateHierarchyTreeView()
        {
            UpdateModelHierarchy();

            hierarchyTreeView.Nodes.Clear();
            hierarchyTreeView.Nodes.Add(ToTreeNode(ModelHierarchy));
            hierarchyTreeView.ExpandAll();
            CheckAll(hierarchyTreeView.Nodes);
        }

        private TreeNode ToTreeNode(PropertyTree tree)
        {
            var node = new TreeNode(tree.Entity.ToString());
            node.Tag = tree;
            var children = from child in tree.Children
                           select ToTreeNode(child);
            node.Nodes.AddRange(children.ToArray());

            return node;
        }

        private PropertyTree ToHierarchy(PropertyTree tree, IEnumerable<TreeNode> nodes)
        {
            //var edges = from child in tree.Children
            //            from node in nodes
            //            where node.Checked && ReferenceEquals(node.Tag, child)
            //            let newTarget = ToHierarchy(child, node.Nodes.Cast<TreeNode>())
            //            select new PropertyTree()
            //            select LabeledTree.Edge<ModelEntity, Unit>(Unit.Instance, newTarget);

            //return LabeledTree.Node(tree.Label, edges);
            return tree;
        }

        private IFilter<PropertyTree> ToFilter(IFilter<PropertyTree> filter, IEnumerable<TreeNode> nodes)
        {
            return filter.TryCast<CompositeFilter<PropertyTree>>().Fold(
                compositeFilter =>
                {
                    var subfilters = from f in compositeFilter.Filters
                                     from node in nodes
                                     where node.Checked && ReferenceEquals(node.Tag, f)
                                     select ToFilter(f, node.Nodes.Cast<TreeNode>());

                    return compositeFilter.Copy(filters: subfilters);
                },
                () => filter);
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
