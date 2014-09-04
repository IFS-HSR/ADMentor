namespace AdAddIn.ExportProblemSpace
{
    partial class TailorSolutionForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.filterTreeView = new System.Windows.Forms.TreeView();
            this.label2 = new System.Windows.Forms.Label();
            this.hierarchyTreeView = new System.Windows.Forms.TreeView();
            this.export = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Solution Filter:";
            // 
            // filterTreeView
            // 
            this.filterTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filterTreeView.Location = new System.Drawing.Point(15, 25);
            this.filterTreeView.Name = "filterTreeView";
            this.filterTreeView.Size = new System.Drawing.Size(667, 165);
            this.filterTreeView.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 193);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Solution:";
            // 
            // hierarchyTreeView
            // 
            this.hierarchyTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hierarchyTreeView.Location = new System.Drawing.Point(15, 209);
            this.hierarchyTreeView.Name = "hierarchyTreeView";
            this.hierarchyTreeView.Size = new System.Drawing.Size(667, 323);
            this.hierarchyTreeView.TabIndex = 3;
            // 
            // export
            // 
            this.export.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.export.Location = new System.Drawing.Point(526, 538);
            this.export.Name = "export";
            this.export.Size = new System.Drawing.Size(75, 23);
            this.export.TabIndex = 4;
            this.export.Text = "Export";
            this.export.UseVisualStyleBackColor = true;
            // 
            // cancel
            // 
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(607, 538);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 5;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            // 
            // TailorSolutionForm
            // 
            this.AcceptButton = this.export;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(694, 573);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.export);
            this.Controls.Add(this.hierarchyTreeView);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.filterTreeView);
            this.Controls.Add(this.label1);
            this.Name = "TailorSolutionForm";
            this.Text = "TailorSolutionForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TreeView filterTreeView;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TreeView hierarchyTreeView;
        private System.Windows.Forms.Button export;
        private System.Windows.Forms.Button cancel;
    }
}