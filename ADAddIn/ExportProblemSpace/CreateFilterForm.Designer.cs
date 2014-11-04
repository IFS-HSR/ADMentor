namespace AdAddIn.ExportProblemSpace
{
    partial class CreateFilterForm
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
            this.fieldBox = new System.Windows.Forms.ComboBox();
            this.operatorBox = new System.Windows.Forms.ComboBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.valueBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // fieldBox
            // 
            this.fieldBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.fieldBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.fieldBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fieldBox.FormattingEnabled = true;
            this.fieldBox.Location = new System.Drawing.Point(12, 12);
            this.fieldBox.Name = "fieldBox";
            this.fieldBox.Size = new System.Drawing.Size(121, 21);
            this.fieldBox.TabIndex = 0;
            this.fieldBox.SelectedIndexChanged += new System.EventHandler(this.fieldBox_SelectedIndexChanged);
            // 
            // operatorBox
            // 
            this.operatorBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.operatorBox.FormattingEnabled = true;
            this.operatorBox.Location = new System.Drawing.Point(139, 12);
            this.operatorBox.Name = "operatorBox";
            this.operatorBox.Size = new System.Drawing.Size(121, 21);
            this.operatorBox.TabIndex = 1;
            this.operatorBox.SelectedIndexChanged += new System.EventHandler(this.operatorBox_SelectedIndexChanged);
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(231, 39);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 3;
            this.buttonOk.Text = "Ok";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(312, 39);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // valueBox
            // 
            this.valueBox.FormattingEnabled = true;
            this.valueBox.Location = new System.Drawing.Point(266, 12);
            this.valueBox.Name = "valueBox";
            this.valueBox.Size = new System.Drawing.Size(121, 21);
            this.valueBox.TabIndex = 2;
            // 
            // CreateFilterForm
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(399, 72);
            this.Controls.Add(this.valueBox);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.operatorBox);
            this.Controls.Add(this.fieldBox);
            this.Name = "CreateFilterForm";
            this.Text = "CreateFilterForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox fieldBox;
        private System.Windows.Forms.ComboBox operatorBox;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ComboBox valueBox;
    }
}