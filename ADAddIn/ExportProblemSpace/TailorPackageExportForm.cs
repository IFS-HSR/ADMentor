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
        private Func<IFilter<ModelEntity>, LabeledTree<ModelEntity, Unit>> GetModelHierarchy = null;
        private CompositeFilter<ModelEntity> OriginalFilter = null;
        private IFilter<ModelEntity> SelectedFilter = null;
        private LabeledTree<ModelEntity, Unit> ModelHierarchy = null;

        public TailorPackageExportForm()
        {
            InitializeComponent();

            filterTreeView.CheckBoxes = true;
            filterTreeView.AfterCheck += filterTreeView_AfterCheck;

            hierarchyTreeView.CheckBoxes = true;
            hierarchyTreeView.AfterCheck += hierarchyTreeView_AfterCheck;
        }

        public Option<LabeledTree<ModelEntity, Unit>> SelectFilter(CompositeFilter<ModelEntity> filter,
            Func<IFilter<ModelEntity>, LabeledTree<ModelEntity, Unit>> getModelHierarchy)
        {
            GetModelHierarchy = getModelHierarchy;
            OriginalFilter = filter;

            filterTreeView.Nodes.Clear();
            var filterNodes = ToTreeNodes(filter.Filters);
            filterTreeView.Nodes.AddRange(filterNodes.ToArray());
            filterTreeView.Nodes.Cast<TreeNode>().ForEach(node =>
            {
                node.Checked = true;
            });

            UpdateHierarchyTreeView();

            var result = ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var hierarchyRoot = hierarchyTreeView.Nodes[0];
                return Options.Some(ToHierarchy(ModelHierarchy, hierarchyRoot.Nodes.Cast<TreeNode>()));
            }
            else
            {
                return Options.None<LabeledTree<ModelEntity, Unit>>();
            }
        }

        private TreeNode[] ToTreeNodes(IEnumerable<IFilter<ModelEntity>> filters)
        {
            return (from filter in filters
                    select filter.TryCast<CompositeFilter<ModelEntity>>().Fold(
                            composite => ToTreeNode(composite),
                            () => ToTreeNode(filter))).ToArray();
        }

        private TreeNode ToTreeNode(CompositeFilter<ModelEntity> filter)
        {
            var node = new TreeNode(filter.Name, ToTreeNodes(filter.Filters));
            node.Tag = filter;

            return node;
        }

        private TreeNode ToTreeNode(IFilter<ModelEntity> filter)
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

        private TreeNode ToTreeNode(LabeledTree<ModelEntity, Unit> tree)
        {
            var node = new TreeNode(tree.Label.ToString());
            node.Tag = tree;
            var children = from edge in tree.Edges
                           select ToTreeNode(edge.Target);
            node.Nodes.AddRange(children.ToArray());

            return node;
        }

        private LabeledTree<ModelEntity, Unit> ToHierarchy(LabeledTree<ModelEntity, Unit> tree, IEnumerable<TreeNode> nodes)
        {
            var edges = from edge in tree.Edges
                        from node in nodes
                        where node.Checked && ReferenceEquals(node.Tag, edge.Target)
                        let newTarget = ToHierarchy(edge.Target, node.Nodes.Cast<TreeNode>())
                        select LabeledTree.Edge<ModelEntity, Unit>(Unit.Instance, newTarget);

            return LabeledTree.Node(tree.Label, edges);
        }

        private IFilter<ModelEntity> ToFilter(IFilter<ModelEntity> filter, IEnumerable<TreeNode> nodes)
        {
            return filter.TryCast<CompositeFilter<ModelEntity>>().Fold(
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
