using AdAddIn.Model;
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

namespace AdAddIn.DecisionDetails
{
    public partial class DecisionDetailView : Form
    {
        private Decision decision;
        private IEnumerable<Alternative> alternatives;
        private IEnumerable<Issue> issues;

        public DecisionDetailView()
        {
            InitializeComponent();
        }

        public Option<Decision> GetModifications(Decision original, IEnumerable<Alternative> alternatives, IEnumerable<Issue> issues)
        {
            decision = original;
            this.issues = issues;
            this.alternatives = alternatives;

            UpdateComponents();

            var result = ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                return decision.AsOption();
            else
                return Options.None<Decision>();
        }

        private void UpdateComponents()
        {
            txtName.Text = decision.Name;

            lstAlternatives.Items.Clear();
            alternatives.ForEach(a =>
            {
                lstAlternatives.Items.Add(a);
                decision.InstantiatesAlternative.Do(altId =>
                {
                    if (a.ID == altId)
                        lstAlternatives.SelectedItem = a;
                });
            });

            lstIssues.Items.Clear();
            issues.ForEach(i =>
            {
                lstIssues.Items.Add(i);
                decision.AddressesIssue.Do(issueId =>
                {
                    if (i.ID == issueId)
                        lstIssues.SelectedItem = i;
                });
            });
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            decision = decision.CopyWith(name: txtName.Text);
        }

        private void lstAlternatives_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedAlternativeId =
                alternatives.ElementAt(lstAlternatives.SelectedIndex).AsOption()
                .Select(a => a.ID);
            decision = decision.CopyWith(instantiatesAlternative: selectedAlternativeId);
        }

        private void lstIssues_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedIssueId =
                issues.ElementAt(lstIssues.SelectedIndex).AsOption()
                .Select(i => i.ID);
            decision = decision.CopyWith(addressesIssue: selectedIssueId);
        }
    }
}
