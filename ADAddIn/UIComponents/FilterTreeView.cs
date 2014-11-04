using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdAddIn.DataAccess;
using Utils;

namespace AdAddIn.UIComponents
{
    public partial class FilterTreeView : UserControl
    {
        private ObservableAtom<Option<ModelFilter>> SelectedFilter = new ObservableAtom<Option<ModelFilter>>(Options.None<ModelFilter>());

        public FilterTreeView()
        {
            InitializeComponent();

            SelectedFilter.AddListener(OnSelectedFilterChanged);

            Filter = new ObservableAtom<ModelFilter>(new ModelFilter.Any());
            Filter.AddListener(OnFilterChanged);
            UpdateTree(Filter.Val);

            CreateNewFilter = () => Options.None<ModelFilter>();
        }

        public ObservableAtom<ModelFilter> Filter { get; private set; }

        public Func<Option<ModelFilter>> CreateNewFilter { get; set; }

        private void OnFilterChanged(ModelFilter filter)
        {
            UpdateTree(filter);
        }

        private void OnSelectedFilterChanged(Option<ModelFilter> selected)
        {
            selected.Match(
                f =>
                {
                    btnAddAlternative.Enabled = true;
                    btnAddRestriction.Enabled = true;
                    btnRemove.Enabled = true;
                },
                () =>
                {
                    btnAddAlternative.Enabled = false;
                    btnAddRestriction.Enabled = false;
                    btnRemove.Enabled = false;
                });
        }

        private void UpdateTree(ModelFilter filter)
        {
            treeView.Nodes.Clear();
            var rootNode = new TreeNode();
            UpdateTree(rootNode, filter);
            treeView.Nodes.Add(rootNode);
            treeView.ExpandAll();
        }

        private void UpdateTree(TreeNode node, ModelFilter filter)
        {
            node.Text = filter.Name;
            node.Tag = filter;

            var subfilters = from composite in filter.Match<ModelFilter.Composite>()
                             from f in composite.Filters
                             select f;

            subfilters.ForEach(f =>
            {
                var child = new TreeNode();
                UpdateTree(child, f);
                node.Nodes.Add(child);
            });
        }

        private void treeView_AfterSelect(object _, TreeViewEventArgs e)
        {
            var filter = e.Node.Tag as ModelFilter;
            SelectedFilter.Exchange(Options.Some(filter), GetType());
        }

        private void btnAddAlternative_Click(object _, EventArgs __)
        {
            SelectedFilter.Val.Do(selectedFilter =>
            {
                CreateNewFilter().Match(newFilter =>
                {
                    Filter.Swap(f => f.AddAlternative(selectedFilter, newFilter), GetType());
                    SelectedFilter.Exchange(Options.None<ModelFilter>(), GetType());
                }, () => { });
            });
        }

        private void btnAddRestriction_Click(object _, EventArgs __)
        {
            SelectedFilter.Val.Do(selectedFilter =>
            {
                CreateNewFilter().Match(newFilter =>
                {
                    Filter.Swap(f => f.AddRestriction(selectedFilter, newFilter), GetType());
                    SelectedFilter.Exchange(Options.None<ModelFilter>(), GetType());
                }, () => { });
            });
        }

        private void btnRemove_Click(object _, EventArgs __)
        {
            SelectedFilter.Val.Do(selectedFilter =>
            {
                Filter.Swap(f => f.Remove(selectedFilter), GetType());
                SelectedFilter.Exchange(Options.None<ModelFilter>(), GetType());
            });
        }
    }
}
