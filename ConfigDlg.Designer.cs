namespace LinkTranslator
{
    partial class ConfigDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose (bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose ();
            }
            base.Dispose (disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent ()
        {
            this.label1 = new System.Windows.Forms.Label ();
            this.txtDocumentFolder = new System.Windows.Forms.TextBox ();
            this.btnBrowseDocumentFolder = new System.Windows.Forms.Button ();
            this.btnOK = new System.Windows.Forms.Button ();
            this.btnCancel = new System.Windows.Forms.Button ();
            this.SuspendLayout ();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point (13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size (91, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Document Folder:";
            // 
            // txtDocumentFolder
            // 
            this.txtDocumentFolder.Location = new System.Drawing.Point (111, 13);
            this.txtDocumentFolder.Name = "txtDocumentFolder";
            this.txtDocumentFolder.Size = new System.Drawing.Size (269, 20);
            this.txtDocumentFolder.TabIndex = 1;
            // 
            // btnBrowseDocumentFolder
            // 
            this.btnBrowseDocumentFolder.Location = new System.Drawing.Point (386, 10);
            this.btnBrowseDocumentFolder.Name = "btnBrowseDocumentFolder";
            this.btnBrowseDocumentFolder.Size = new System.Drawing.Size (68, 23);
            this.btnBrowseDocumentFolder.TabIndex = 2;
            this.btnBrowseDocumentFolder.Text = "Browse ...";
            this.btnBrowseDocumentFolder.UseVisualStyleBackColor = true;
            this.btnBrowseDocumentFolder.Click += new System.EventHandler (this.btnBrowseDocumentFolder_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point (298, 215);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size (75, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler (this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point (379, 215);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size (75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // ConfigDlg
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size (466, 250);
            this.Controls.Add (this.btnCancel);
            this.Controls.Add (this.btnOK);
            this.Controls.Add (this.btnBrowseDocumentFolder);
            this.Controls.Add (this.txtDocumentFolder);
            this.Controls.Add (this.label1);
            this.Name = "ConfigDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Link Translator - Configuration";
            this.ResumeLayout (false);
            this.PerformLayout ();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDocumentFolder;
        private System.Windows.Forms.Button btnBrowseDocumentFolder;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}