namespace AdAddIn.ExportProblemSpace
{
    partial class ElementFilterConfiguration
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
            this.filterTreeView1 = new AdAddIn.UIComponents.FilterTreeView();
            this.SuspendLayout();
            // 
            // filterTreeView1
            // 
            this.filterTreeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filterTreeView1.Location = new System.Drawing.Point(12, 12);
            this.filterTreeView1.Name = "filterTreeView1";
            this.filterTreeView1.Size = new System.Drawing.Size(530, 353);
            this.filterTreeView1.TabIndex = 0;
            // 
            // ElementFilterConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 377);
            this.Controls.Add(this.filterTreeView1);
            this.Name = "ElementFilterConfiguration";
            this.Text = "ElementFilterConfiguration";
            this.ResumeLayout(false);

        }

        #endregion

        private AdAddIn.UIComponents.FilterTreeView filterTreeView1;
    }
}