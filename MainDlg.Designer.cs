namespace LinkTranslator
{
    partial class MainDlg
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
            this.btnTransLinks = new System.Windows.Forms.Button ();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip ();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.configureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.collectLinksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.showHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.label1 = new System.Windows.Forms.Label ();
            this.btnClose = new System.Windows.Forms.Button ();
            this.label2 = new System.Windows.Forms.Label ();
            this.label3 = new System.Windows.Forms.Label ();
            this.txtEnURL = new System.Windows.Forms.TextBox ();
            this.txtEnText = new System.Windows.Forms.TextBox ();
            this.label4 = new System.Windows.Forms.Label ();
            this.label5 = new System.Windows.Forms.Label ();
            this.txtDeURL = new System.Windows.Forms.TextBox ();
            this.txtDeText = new System.Windows.Forms.TextBox ();
            this.btnPrevious = new System.Windows.Forms.Button ();
            this.btnNext = new System.Windows.Forms.Button ();
            this.lblNumber = new System.Windows.Forms.Label ();
            this.label7 = new System.Windows.Forms.Label ();
            this.label8 = new System.Windows.Forms.Label ();
            this.btnAppendToDoc = new System.Windows.Forms.Button ();
            this.statusStrip = new System.Windows.Forms.StatusStrip ();
            this.stsAction = new System.Windows.Forms.ToolStripStatusLabel ();
            this.btnShowEnUrl = new System.Windows.Forms.Button ();
            this.btnShowDeUrl = new System.Windows.Forms.Button ();
            this.label6 = new System.Windows.Forms.Label ();
            this.lblTransMethod = new System.Windows.Forms.Label ();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.menuStrip1.SuspendLayout ();
            this.statusStrip.SuspendLayout ();
            this.SuspendLayout ();
            // 
            // btnTransLinks
            // 
            this.btnTransLinks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnTransLinks.Location = new System.Drawing.Point (18, 344);
            this.btnTransLinks.Name = "btnTransLinks";
            this.btnTransLinks.Size = new System.Drawing.Size (248, 23);
            this.btnTransLinks.TabIndex = 0;
            this.btnTransLinks.Text = "Translate Links in OpenOffice Document ...";
            this.btnTransLinks.UseVisualStyleBackColor = true;
            this.btnTransLinks.Click += new System.EventHandler (this.btnTransLinks_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange (new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point (0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size (634, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange (new System.Windows.Forms.ToolStripItem[] {
            this.configureToolStripMenuItem,
            this.collectLinksToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size (47, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // configureToolStripMenuItem
            // 
            this.configureToolStripMenuItem.Name = "configureToolStripMenuItem";
            this.configureToolStripMenuItem.Size = new System.Drawing.Size (153, 22);
            this.configureToolStripMenuItem.Text = "Configure ...";
            this.configureToolStripMenuItem.Click += new System.EventHandler (this.configureToolStripMenuItem_Click);
            // 
            // collectLinksToolStripMenuItem
            // 
            this.collectLinksToolStripMenuItem.Name = "collectLinksToolStripMenuItem";
            this.collectLinksToolStripMenuItem.Size = new System.Drawing.Size (153, 22);
            this.collectLinksToolStripMenuItem.Text = "Collect Links ...";
            this.collectLinksToolStripMenuItem.Click += new System.EventHandler (this.collectLinksToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange (new System.Windows.Forms.ToolStripItem[] {
            this.showHelpToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size (44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // showHelpToolStripMenuItem
            // 
            this.showHelpToolStripMenuItem.Name = "showHelpToolStripMenuItem";
            this.showHelpToolStripMenuItem.Size = new System.Drawing.Size (131, 22);
            this.showHelpToolStripMenuItem.Text = "Show Help";
            this.showHelpToolStripMenuItem.Click += new System.EventHandler (this.showHelpToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size (131, 22);
            this.aboutToolStripMenuItem.Text = "About ...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler (this.aboutToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point (123, 175);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size (413, 14);
            this.label1.TabIndex = 4;
            this.label1.Text = "You may drag and drop OpenOffice files and single hyperlinks here!";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point (550, 344);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size (75, 23);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler (this.btnClose_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point (12, 125);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size (32, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "URL:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point (12, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size (79, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Text reference:";
            // 
            // txtEnURL
            // 
            this.txtEnURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEnURL.Location = new System.Drawing.Point (97, 125);
            this.txtEnURL.Name = "txtEnURL";
            this.txtEnURL.Size = new System.Drawing.Size (455, 20);
            this.txtEnURL.TabIndex = 8;
            // 
            // txtEnText
            // 
            this.txtEnText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEnText.Location = new System.Drawing.Point (97, 87);
            this.txtEnText.Name = "txtEnText";
            this.txtEnText.Size = new System.Drawing.Size (455, 20);
            this.txtEnText.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point (12, 257);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size (32, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "URL:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point (12, 219);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size (79, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Text reference:";
            // 
            // txtDeURL
            // 
            this.txtDeURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDeURL.Location = new System.Drawing.Point (97, 257);
            this.txtDeURL.Name = "txtDeURL";
            this.txtDeURL.Size = new System.Drawing.Size (455, 20);
            this.txtDeURL.TabIndex = 10;
            this.txtDeURL.TextChanged += new System.EventHandler (this.txtDeURL_TextChanged);
            // 
            // txtDeText
            // 
            this.txtDeText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDeText.Location = new System.Drawing.Point (97, 219);
            this.txtDeText.Name = "txtDeText";
            this.txtDeText.Size = new System.Drawing.Size (455, 20);
            this.txtDeText.TabIndex = 9;
            this.txtDeText.TextChanged += new System.EventHandler (this.txtDeText_TextChanged);
            // 
            // btnPrevious
            // 
            this.btnPrevious.Location = new System.Drawing.Point (154, 50);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size (75, 23);
            this.btnPrevious.TabIndex = 3;
            this.btnPrevious.Text = "previous";
            this.btnPrevious.UseVisualStyleBackColor = true;
            this.btnPrevious.Click += new System.EventHandler (this.btnPrevious_Click);
            // 
            // btnNext
            // 
            this.btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNext.Location = new System.Drawing.Point (415, 50);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size (75, 23);
            this.btnNext.TabIndex = 4;
            this.btnNext.Text = "next";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler (this.btnNext_Click);
            // 
            // lblNumber
            // 
            this.lblNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNumber.Location = new System.Drawing.Point (253, 55);
            this.lblNumber.Name = "lblNumber";
            this.lblNumber.Size = new System.Drawing.Size (140, 18);
            this.lblNumber.TabIndex = 17;
            this.lblNumber.Text = "n / m";
            this.lblNumber.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point (15, 50);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size (52, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "English:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point (15, 189);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size (54, 13);
            this.label8.TabIndex = 19;
            this.label8.Text = "German:";
            // 
            // btnAppendToDoc
            // 
            this.btnAppendToDoc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAppendToDoc.Location = new System.Drawing.Point (272, 344);
            this.btnAppendToDoc.Name = "btnAppendToDoc";
            this.btnAppendToDoc.Size = new System.Drawing.Size (156, 23);
            this.btnAppendToDoc.TabIndex = 1;
            this.btnAppendToDoc.Text = "Append Links toDocument";
            this.btnAppendToDoc.UseVisualStyleBackColor = true;
            this.btnAppendToDoc.Click += new System.EventHandler (this.btnAppendToDoc_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange (new System.Windows.Forms.ToolStripItem[] {
            this.stsAction});
            this.statusStrip.Location = new System.Drawing.Point (0, 379);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size (634, 22);
            this.statusStrip.TabIndex = 21;
            this.statusStrip.Text = "statusStrip1";
            // 
            // stsAction
            // 
            this.stsAction.Name = "stsAction";
            this.stsAction.Size = new System.Drawing.Size (40, 17);
            this.stsAction.Text = "action";
            // 
            // btnShowEnUrl
            // 
            this.btnShowEnUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowEnUrl.Location = new System.Drawing.Point (558, 125);
            this.btnShowEnUrl.Name = "btnShowEnUrl";
            this.btnShowEnUrl.Size = new System.Drawing.Size (67, 23);
            this.btnShowEnUrl.TabIndex = 5;
            this.btnShowEnUrl.Text = "Show";
            this.btnShowEnUrl.UseVisualStyleBackColor = true;
            this.btnShowEnUrl.Click += new System.EventHandler (this.btnShowEnUrl_Click);
            // 
            // btnShowDeUrl
            // 
            this.btnShowDeUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowDeUrl.Location = new System.Drawing.Point (558, 255);
            this.btnShowDeUrl.Name = "btnShowDeUrl";
            this.btnShowDeUrl.Size = new System.Drawing.Size (67, 23);
            this.btnShowDeUrl.TabIndex = 6;
            this.btnShowDeUrl.Text = "Show";
            this.btnShowDeUrl.UseVisualStyleBackColor = true;
            this.btnShowDeUrl.Click += new System.EventHandler (this.btnShowDeUrl_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point (97, 295);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size (100, 13);
            this.label6.TabIndex = 22;
            this.label6.Text = "Translation method:";
            // 
            // lblTransMethod
            // 
            this.lblTransMethod.AutoSize = true;
            this.lblTransMethod.Location = new System.Drawing.Point (203, 295);
            this.lblTransMethod.Name = "lblTransMethod";
            this.lblTransMethod.Size = new System.Drawing.Size (0, 13);
            this.lblTransMethod.TabIndex = 23;
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange (new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size (39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size (152, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler (this.pasteToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size (152, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler (this.copyToolStripMenuItem_Click);
            // 
            // MainDlg
            // 
            this.AcceptButton = this.btnClose;
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size (634, 401);
            this.Controls.Add (this.lblTransMethod);
            this.Controls.Add (this.label6);
            this.Controls.Add (this.btnShowDeUrl);
            this.Controls.Add (this.btnShowEnUrl);
            this.Controls.Add (this.statusStrip);
            this.Controls.Add (this.btnAppendToDoc);
            this.Controls.Add (this.label8);
            this.Controls.Add (this.label7);
            this.Controls.Add (this.lblNumber);
            this.Controls.Add (this.btnNext);
            this.Controls.Add (this.btnPrevious);
            this.Controls.Add (this.label4);
            this.Controls.Add (this.label5);
            this.Controls.Add (this.txtDeURL);
            this.Controls.Add (this.txtDeText);
            this.Controls.Add (this.label2);
            this.Controls.Add (this.label3);
            this.Controls.Add (this.txtEnURL);
            this.Controls.Add (this.txtEnText);
            this.Controls.Add (this.btnClose);
            this.Controls.Add (this.label1);
            this.Controls.Add (this.btnTransLinks);
            this.Controls.Add (this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size (577, 407);
            this.Name = "MainDlg";
            this.Text = "Link Translator";
            this.DragDrop += new System.Windows.Forms.DragEventHandler (this.MainDlg_DragDrop);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler (this.MainDlg_MouseDown);
            this.DragEnter += new System.Windows.Forms.DragEventHandler (this.MainDlg_DragEnter);
            this.menuStrip1.ResumeLayout (false);
            this.menuStrip1.PerformLayout ();
            this.statusStrip.ResumeLayout (false);
            this.statusStrip.PerformLayout ();
            this.ResumeLayout (false);
            this.PerformLayout ();

        }

        #endregion

        private System.Windows.Forms.Button btnTransLinks;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem configureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collectLinksToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtEnURL;
        private System.Windows.Forms.TextBox txtEnText;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtDeURL;
        private System.Windows.Forms.TextBox txtDeText;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Label lblNumber;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnAppendToDoc;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel stsAction;
        private System.Windows.Forms.Button btnShowEnUrl;
        private System.Windows.Forms.Button btnShowDeUrl;
        private System.Windows.Forms.ToolStripMenuItem showHelpToolStripMenuItem;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblTransMethod;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
    }
}

