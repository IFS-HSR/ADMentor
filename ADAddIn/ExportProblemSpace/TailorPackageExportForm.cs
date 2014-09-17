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
        private Atom<Func<IFilter<ModelEntity>, LabeledTree<ModelEntity, Unit>>> GetModelHierarchy = new Atom<Func<IFilter<ModelEntity>, LabeledTree<ModelEntity, Unit>>>(null);
        private Atom<CompositeFilter<ModelEntity>> OriginalFilter = new Atom<CompositeFilter<ModelEntity>>(null);
        private Atom<IFilter<ModelEntity>> SelectedFilter = new Atom<IFilter<ModelEntity>>(null);
        private Atom<LabeledTree<ModelEntity, Unit>> ModelHierarchy = new Atom<LabeledTree<ModelEntity, Unit>>(null);

        public TailorPackageExportForm()
        {
            InitializeComponent();

            filterTreeView.CheckBoxes = true;
            filterTreeView.AfterCheck += filterTreeView_AfterCheck;

            hierarchyTreeView.AfterCheck += hierarchyTreeView_AfterCheck;
        }

        public Option<LabeledTree<ModelEntity, Unit>> SelectFilter(CompositeFilter<ModelEntity> filter,
            Func<IFilter<ModelEntity>, LabeledTree<ModelEntity, Unit>> getModelHierarchy)
        {
            GetModelHierarchy.Exchange(getModelHierarchy, GetType());
            OriginalFilter.Exchange(filter, GetType());

            filterTreeView.Nodes.Clear();
            var filterNodes = ToTreeNodes(filter.Filters);
            filterTreeView.Nodes.AddRange(filterNodes.ToArray());

            UpdateHierarchyTreeView();

            var result = ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                return Options.Some(ModelHierarchy.Val);
            }
            else
            {
                return Options.None<LabeledTree<ModelEntity, Unit>>();
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

        private void UpdateSelectedFilter()
        {
            var filter = ToFilter(OriginalFilter.Val, filterTreeView.Nodes.Cast<TreeNode>());

            SelectedFilter.Exchange(filter, GetType());
        }

        private void UpdateModelHierarchy()
        {
            UpdateSelectedFilter();

            var hierarchy = GetModelHierarchy.Val(SelectedFilter.Val);

            ModelHierarchy.Exchange(hierarchy, GetType());
        }

        private void UpdateHierarchyTreeView()
        {
            UpdateModelHierarchy();

            hierarchyTreeView.Nodes.Clear();
            hierarchyTreeView.Nodes.Add(ToTreeNode(ModelHierarchy.Val));
            hierarchyTreeView.ExpandAll();
        }

        private TreeNode ToTreeNode(LabeledTree<ModelEntity, Unit> tree)
        {
            var node = new TreeNode(tree.Label.ToString());
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
