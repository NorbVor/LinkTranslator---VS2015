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
            txtTransDbFolder.Text = Properties.Settings.Default.appDataFolderPath;
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

        private void btnBrowseTransDbFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "Set the folder of the translation database files";
            dlg.SelectedPath = txtTransDbFolder.Text;
            dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            DialogResult res = dlg.ShowDialog();
            if (res == DialogResult.OK)
                txtTransDbFolder.Text = dlg.SelectedPath;
        }

        private void btnOK_Click (object sender, EventArgs e)
        {
            // check that this is indeed a directory
            if (!CheckFolder(txtDocumentFolder.Text, false) ||
                !CheckFolder(txtTransDbFolder.Text, true))
                return;

            // save into settings
            Properties.Settings.Default.documentFolderPath = txtDocumentFolder.Text;
            Properties.Settings.Default.appDataFolderPath = txtTransDbFolder.Text;
            Properties.Settings.Default.Save ();

            DialogResult = DialogResult.OK;
        }

        private bool CheckFolder (string folderPath, bool create)
        {
            if (Directory.Exists(folderPath))
                return true;
            if (create)
            {
                if (MessageBox.Show(this, $"Folder {folderPath} does not exist. Create that folder?",
                    "Folder creation", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return false;
                try
                {
                    Directory.CreateDirectory(folderPath);
                }
                catch
                {
                    MessageBox.Show(this, $"Folder {folderPath} could not be created.", "Folder creation failed", MessageBoxButtons.OK);
                    return false;
                }
                return true;
            }
            else
            {
                MessageBox.Show(this, $"Folder {folderPath} does not exist.", "Invalid folder name", MessageBoxButtons.OK);
            }
            return false;
        }
    }
}