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
        private Atom<Func<IFilter<ModelEntity>, LabeledTree<ModelEntity, Unit>>> getModelHierarchy;
        private Atom<CompositeFilter<ModelEntity>> rootFilter;

        public TailorPackageExportForm()
        {
            getModelHierarchy = new Atom<Func<IFilter<ModelEntity>, LabeledTree<ModelEntity, Unit>>>(null);
            rootFilter = new Atom<CompositeFilter<ModelEntity>>(null);

            InitializeComponent();

            filterTreeView.CheckBoxes = true;
            filterTreeView.AfterCheck += filterTreeView_AfterCheck;

            hierarchyTreeView.AfterCheck += hierarchyTreeView_AfterCheck;
        }

        public Option<IFilter<ModelEntity>> SelectFilter(CompositeFilter<ModelEntity> filter,
            Func<IFilter<ModelEntity>, LabeledTree<ModelEntity, Unit>> getModelHierarchy)
        {
            this.getModelHierarchy.Exchange(getModelHierarchy, GetType());
            rootFilter.Exchange(filter, GetType());

            filterTreeView.Nodes.Clear();
            var filterNodes = ToTreeNodes(filter.Filters);
            filterTreeView.Nodes.AddRange(filterNodes.ToArray());

            UpdateHierarchyTreeView();

            var result = ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                return Options.Some(ToFilter(rootFilter.Val, filterTreeView.Nodes.Cast<TreeNode>()));
            }
            else
            {
                return Options.None<CompositeFilter<ModelEntity>>();
            }
        }

        private TreeNode[] ToTreeNodes(IEnumerable<IFilter<ModelEntity>> filters)
        {
            return (from filter in filters
                    select filter.Match<CompositeFilter<ModelEntity>>().Match(
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

        private void UpdateHierarchyTreeView()
        {
            var filter = ToFilter(rootFilter.Val, filterTreeView.Nodes.Cast<TreeNode>());
            var hierarchy = getModelHierarchy.Val(filter);

            hierarchyTreeView.Nodes.Clear();
            hierarchyTreeView.Nodes.Add(ToTreeNode(hierarchy));
            hierarchyTreeView.ExpandAll();
        }

        private TreeNode ToTreeNode(LabeledTree<ModelEntity, Unit> tree)
        {
            var node = new TreeNode(tree.Label.Name);
            var children = from edge in tree.Edges
                           select ToTreeNode(edge.Target);
            node.Nodes.AddRange(children.ToArray());

            return node;
        }

        private IFilter<ModelEntity> ToFilter(IFilter<ModelEntity> filter, IEnumerable<TreeNode> nodes)
        {
            return filter.Match<CompositeFilter<ModelEntity>>().Match(
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
