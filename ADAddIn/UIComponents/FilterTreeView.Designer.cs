namespace AdAddIn.UIComponents
{
    partial class FilterTreeView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilterTreeView));
            this.treeView = new System.Windows.Forms.TreeView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnAddAlternative = new System.Windows.Forms.ToolStripButton();
            this.btnAddRestriction = new System.Windows.Forms.ToolStripButton();
            this.btnRemove = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView.Location = new System.Drawing.Point(0, 28);
            this.treeView.Name = "treeView";
            this.treeView.ShowPlusMinus = false;
            this.treeView.Size = new System.Drawing.Size(339, 198);
            this.treeView.TabIndex = 0;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnAddAlternative,
            this.btnAddRestriction,
            this.btnRemove});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(339, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnAddAlternative
            // 
            this.btnAddAlternative.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAddAlternative.Image = ((System.Drawing.Image)(resources.GetObject("btnAddAlternative.Image")));
            this.btnAddAlternative.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddAlternative.Name = "btnAddAlternative";
            this.btnAddAlternative.Size = new System.Drawing.Size(93, 22);
            this.btnAddAlternative.Text = "Add Alternative";
            this.btnAddAlternative.Click += new System.EventHandler(this.btnAddAlternative_Click);
            // 
            // btnAddRestriction
            // 
            this.btnAddRestriction.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAddRestriction.Image = ((System.Drawing.Image)(resources.GetObject("btnAddRestriction.Image")));
            this.btnAddRestriction.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddRestriction.Name = "btnAddRestriction";
            this.btnAddRestriction.Size = new System.Drawing.Size(92, 22);
            this.btnAddRestriction.Text = "Add Restriction";
            this.btnAddRestriction.Click += new System.EventHandler(this.btnAddRestriction_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnRemove.Image = ((System.Drawing.Image)(resources.GetObject("btnRemove.Image")));
            this.btnRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(80, 22);
            this.btnRemove.Text = "Remove Rule";
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // FilterTreeView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.treeView);
            this.Name = "FilterTreeView";
            this.Size = new System.Drawing.Size(339, 226);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnAddAlternative;
        private System.Windows.Forms.ToolStripButton btnAddRestriction;
        private System.Windows.Forms.ToolStripButton btnRemove;
    }
}
