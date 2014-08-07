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

namespace AdAddIn.PopulateDependencies
{
    public partial class DependencySelectorForm : Form, IDependencySelector
    {
        public DependencySelectorForm()
        {
            InitializeComponent();

            dependencyTreeView.CheckBoxes = true;
        }

        public Option<DependencyTree.Node> GetSelectedDependencies(DependencyTree.Node availableDependencies, IEnumerable<EA.Element> alreadyAddedElements)
        {
            dependencyTreeView.Nodes.Clear();
            dependencyTreeView.Nodes.Add(ToTreeNode(availableDependencies, alreadyAddedElements));
            dependencyTreeView.ExpandAll();

            ShowDialog();

            return Options.None<DependencyTree.Node>();
        }

        private TreeNode ToTreeNode(DependencyTree.Node dependencies, IEnumerable<EA.Element> alreadyAddedElements)
        {
            var node = new TreeNode(dependencies.Element.Name, ToTreeNodes(dependencies.Children, alreadyAddedElements));
            if (alreadyAddedElements.Any(e => e.ElementID == dependencies.Element.ElementID))
                node.ForeColor = System.Drawing.SystemColors.GrayText;
            return node;
        }

        private TreeNode[] ToTreeNodes(IEnumerable<DependencyTree.Edge> children, IEnumerable<EA.Element> alreadyAddedElements)
        {
            return children.Select(edge =>
            {
                var label = String.Format("{0}: {1}", edge.Connector.Stereotype, edge.Node.Element.Name);
                var node = new TreeNode(label, ToTreeNodes(edge.Node.Children, alreadyAddedElements));
                if (alreadyAddedElements.Any(e => e.ElementID == edge.Node.Element.ElementID))
                    node.ForeColor = System.Drawing.SystemColors.GrayText;
                return node;
            }).ToArray();
        }
    }
}
