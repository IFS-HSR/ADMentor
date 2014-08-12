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

        public Option<LabeledTree<EA.Element, EA.Connector>> GetSelectedDependencies(LabeledTree<EA.Element, EA.Connector> availableDependencies, IEnumerable<EA.Element> alreadyAddedElements)
        {
            dependencyTreeView.Nodes.Clear();
            dependencyTreeView.Nodes.Add(ToTreeNode(availableDependencies, alreadyAddedElements));
            dependencyTreeView.ExpandAll();

            ShowDialog();

            return Options.None<LabeledTree<EA.Element, EA.Connector>>();
        }

        private TreeNode ToTreeNode(LabeledTree<EA.Element, EA.Connector> dependencies, IEnumerable<EA.Element> alreadyAddedElements)
        {
            var node = new TreeNode(dependencies.Label.Name, ToTreeNodes(dependencies.Edges, alreadyAddedElements));
            node.Tag = dependencies;
            if (alreadyAddedElements.Any(e => e.ElementID == dependencies.Label.ElementID))
                node.ForeColor = System.Drawing.SystemColors.GrayText;
            return node;
        }

        private TreeNode[] ToTreeNodes(IEnumerable<LabeledTree<EA.Element, EA.Connector>.Edge> edges, IEnumerable<EA.Element> alreadyAddedElements)
        {
            return edges.Select(edge =>
            {
                var label = String.Format("{0}: {1}", edge.Label.Stereotype, edge.Target.Label.Name);
                var node = new TreeNode(label, ToTreeNodes(edge.Target.Edges, alreadyAddedElements));
                node.Tag = edge.Target;
                if (alreadyAddedElements.Any(e => e.ElementID == edge.Target.Label.ElementID))
                    node.ForeColor = System.Drawing.SystemColors.GrayText;
                return node;
            }).ToArray();
        }
    }
}
