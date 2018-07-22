using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace LinkTranslator
{
    public partial class ConfigDlg : Form
    {
        public ConfigDlg ()
        {
            InitializeComponent ();
            RetrieveCurrentSettings ();
        }

        private void RetrieveCurrentSettings ()
        {
            txtDocumentFolder.Text = Properties.Settings.Default.documentFolderPath;
        }

        private void btnBrowseDocumentFolder_Click (object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog ();
            dlg.Description = "Set the start folder for the document open dialog";
            dlg.SelectedPath = txtDocumentFolder.Text;
            dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            DialogResult res = dlg.ShowDialog ();
            if (res == DialogResult.OK)
                txtDocumentFolder.Text = dlg.SelectedPath;
        }

        private void btnOK_Click (object sender, EventArgs e)
        {
            string docFolder = txtDocumentFolder.Text;

            // check that this is indeed a directory
            if (!Directory.Exists (docFolder))
            {
                string msg = string.Format (
                    "The entered folder name \"{0}\" is not a valid directory.",
                    docFolder);
                MessageBox.Show (msg, "Invalid folder name", MessageBoxButtons.OK);
                return;
            }

            // save into settings
            Properties.Settings.Default.documentFolderPath = docFolder;
            Properties.Settings.Default.Save ();

            DialogResult = DialogResult.OK;
        }
    }
}