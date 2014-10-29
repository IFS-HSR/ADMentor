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
        private Atom<Option<ModelFilter>> SelectedFilter = new Atom<Option<ModelFilter>>(Options.None<ModelFilter>());

        public FilterTreeView()
        {
            InitializeComponent();

            Filter = new ObservableAtom<ModelFilter>(new ModelFilter.Any());
            Filter.AddListener(OnFilterChanged);
            UpdateTree(Filter.Val);

            NewFilter = new Atom<Func<ModelFilter>>(() => new ModelFilter.Any());
        }

        public ObservableAtom<ModelFilter> Filter { get; private set; }

        private void OnFilterChanged(ModelFilter filter)
        {
            UpdateTree(filter);
        }

        public Atom<Func<ModelFilter>> NewFilter { get; private set; }

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
                var alternative = NewFilter.Val();
                var newFilter = Filter.Val.AddAlternative(selectedFilter, alternative);

                Filter.Exchange(newFilter, GetType());
            });
        }

        private void btnAddRestriction_Click(object sender, EventArgs e)
        {
            SelectedFilter.Val.Do(selectedFilter =>
            {
                var alternative = NewFilter.Val();
                var newFilter = Filter.Val.AddRestriction(selectedFilter, alternative);

                Filter.Exchange(newFilter, GetType());
            });
        }
    }
}
