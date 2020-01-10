using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using Ionic.Zip;
using System.Collections;
using LinkTranslator.Properties;
using System.Threading;
using System.Globalization;

//TODO: Installer

namespace LinkTranslator
{
    public partial class MainDlg : Form
    {
        private string _filePath;
        private bool _readOnlyMode;
        private List<Hyperlink> _enLinks;
        private List<Hyperlink> _deLinks;
        private LinkTranslator _lt = null;
        private int _curIdx = -1;
        OpenOfficeDoc _doc;
        bool _emitingDragDrop = false;
        bool _isInitializing = true;

        public MainDlg ()
        {
            Helpers.CheckSettingsUpgrade();
            InitializeComponent();
            CheckAppFolderAccess();

            // initialize dialog fields
            lblNumber.Text = "";
            stsAction.Text = "";
            if (_readOnlyMode)
                Text += " (read-only mode)";

            // creat the link translator object
            _lt = new LinkTranslator();

            // create the two link listes for English and German links
            _enLinks = new List<Hyperlink>();
            _deLinks = new List<Hyperlink>();

            // display an empty window
            _curIdx = -1;
            ShowCurrentLink ();

            // we enable that option only when we have read an OpenOffice document to which
            // we can store the translated links
            appendLinksToDocumentToolStripMenuItem.Enabled = false;
        }

        /**********************************************************************
        *** Event Handlers
        **********************************************************************/

        private void translateLinksInOpenOfficeDocumentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string documentFolderPath = Properties.Settings.Default.documentFolderPath;

            // find the newest file in the document directory in order to
            // preselect it in the file open dialog
            string preselFileName = NewestFileofDirectory(documentFolderPath);

#if DEBUG
            documentFolderPath = "";
            preselFileName = "OpenOfficeDoc.odt";
#endif

            // open a file dialog to ask for the file to work on
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = documentFolderPath;
            dlg.Filter = "OpenOffice files (*.odt)|*.odt|All files (*.*)|*.*";
            if (preselFileName != null)
                dlg.FileName = preselFileName;
            dlg.RestoreDirectory = true;
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            ProcessFile(dlg.FileName);
        }


        // append the translated hypelinks at the end of the document
        private void appendLinksToDocumentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_doc == null || _filePath == null)
                return;
            stsAction.Text = "Appending hyperlinks to document";
            if (!_doc.AppendLinksToDoc(_deLinks))
                return;

            // save the document back to disk
            string outputPath = _filePath;
            stsAction.Text = "Saving file: " + _filePath;
#if DEBUG
            outputPath = "newDoc.odt";
#endif
            _doc.Save(outputPath);
            stsAction.Text = "Done";

            _doc = null;
            _filePath = null;
            ClearAllLinks();
            UpdateDialogTitle();
            appendLinksToDocumentToolStripMenuItem.Enabled = false;
        }

        private void btnPrevious_Click (object sender, EventArgs e)
        {
            _curIdx -= 1;
            if (_curIdx < 0)
                _curIdx = 0;
            ShowCurrentLink ();
        }

        private void btnNext_Click (object sender, EventArgs e)
        {
            _curIdx += 1;
            if (_curIdx > _enLinks.Count - 1)
                _curIdx = _enLinks.Count - 1;
            ShowCurrentLink ();
        }

        private void btnShowDeUrl_Click (object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start (txtDeURL.Text);
        }

        private void btnShowEnUrl_Click (object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start (txtEnURL.Text);
        }

        private void txtDeText_TextChanged (object sender, EventArgs e)
        {
            if (_curIdx < 0 || _isInitializing)
                return;
            _deLinks[_curIdx].textChanged = true;
            btnAddTextToDB.Enabled = !_readOnlyMode;
            _deLinks[_curIdx].text = txtDeText.Text;
        }

        private void txtDeURL_TextChanged (object sender, EventArgs e)
        {
            if (_curIdx < 0 || _isInitializing)
                return;
            _deLinks[_curIdx].uriChanged = true;
            btnAddURLtoDB.Enabled = !_readOnlyMode;
            _deLinks[_curIdx].uri = txtDeURL.Text;
        }

        /*
         * Tool Strip Event Handlers
         */
        private void configureToolStripMenuItem_Click (object sender, EventArgs e)
        {
            // open the configuration dialog
            ConfigDlg dlg = new ConfigDlg ();
            if (dlg.ShowDialog() == DialogResult.OK)
                _lt.ReloadDatabase();
        }

        private void aboutToolStripMenuItem_Click (object sender, EventArgs e)
        {
            // show About dialog
            AboutBox dlg = new AboutBox ();
            dlg.ShowDialog (this);
        }

        private void showHelpToolStripMenuItem_Click (object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("acrord32.exe", $"/A \"zoom=100\" " + "LinkTranslatorHelp.pdf");
        }

        private void btnAddTextToDB_Click(object sender, EventArgs e)
        {
            if (_curIdx < 0)
                return;
            _lt.AddTextTranslation(txtEnText.Text, txtDeText.Text);

            // re-translate the current link with the amended database and reset changed-flag.
            // but keep any changes to the URL!
            bool savedModifiedUri = false;
            string modifiedUri = null;
            if (_deLinks[_curIdx].uriChanged)
            {
                modifiedUri = _deLinks[_curIdx].uri;
                savedModifiedUri = true;
            }
            _deLinks[_curIdx] = _lt.TranslateLink2(_enLinks[_curIdx]);
            if (savedModifiedUri)
            {
                _deLinks[_curIdx].uri = modifiedUri;
                _deLinks[_curIdx].uriChanged = true;
            }
            ShowCurrentLink();
        }

        private void btnAddURLtoDB_Click(object sender, EventArgs e)
        {
            if (_curIdx < 0)
                return;
            _lt.AddUrlTranslation(txtEnURL.Text, txtDeURL.Text);

            // re-translate the current link with the amended database and reset changed-flag.
            bool savedModifiedText = false;
            string modifiedText = null;
            if (_deLinks[_curIdx].textChanged)
            {
                modifiedText = _deLinks[_curIdx].text;
                savedModifiedText = true;
            }
            _deLinks[_curIdx] = _lt.TranslateLink2(_enLinks[_curIdx]);
            if (savedModifiedText)
            {
                _deLinks[_curIdx].text = modifiedText;
                _deLinks[_curIdx].textChanged = true;
            }
            ShowCurrentLink();
        }

        private void btnClose_Click (object sender, EventArgs e)
        {
            Application.Exit ();
        }


        /*
         * Clipboard -- Copy and Paste
         */

        private void copyToolStripMenuItem_Click (object sender, EventArgs e)
        {
            DataObject dataObject;

            // make certain that a valid link is in the German fields
            if (string.IsNullOrEmpty (txtDeURL.Text))
                return;

            // generate a clipboard envelope
            string linkHtml = BuildLinkTag (txtDeText.Text, txtDeURL.Text);
            string env = ClipboardEnvelope.BuildHtmlClipboardEnvelope (linkHtml);
            dataObject = new DataObject ();
            dataObject.SetData ("HTML Format", env);

            // put the data on the clipbaord
            Clipboard.SetDataObject (dataObject);
        }

        private void pasteToolStripMenuItem_Click (object sender, EventArgs e)
        {
            Object dataObj;
            if ((dataObj = Clipboard.GetData (DataFormats.Html)) != null)
                DropHTMLText ((string)dataObj);
            else if ((dataObj = Clipboard.GetData (DataFormats.FileDrop)) != null)
                DropFiles ((string[])Clipboard.GetData (DataFormats.FileDrop));
        }

        private void cbReduceESOLinks_CheckedChanged(object sender, EventArgs e)
        {
            // save new setting
            Settings.Default.reduceESOLinks = cbReduceESOLinks.Checked;

            // re-translate all links
            _lt.ReTranslateESOLinks (_enLinks, _deLinks);
            _isInitializing = true;
            txtDeURL.Text = _deLinks[_curIdx].uri;
            _isInitializing = false;
        }


        /*
         * Drag and Drop Event Handlers
         */

        /// <summary>
        /// Drag and Drop Functionality
        /// </summary>
        private void MainDlg_DragEnter (object sender, DragEventArgs e)
        {
            if (_emitingDragDrop)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            if (e.Data.GetDataPresent (DataFormats.Html) ||
                e.Data.GetDataPresent (DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void MainDlg_DragDrop (object sender, DragEventArgs e)
        {
            if (_emitingDragDrop)
                return;

            if (e.Data.GetDataPresent (DataFormats.FileDrop))
                DropFiles ((string[])(e.Data.GetData (DataFormats.FileDrop)));
            else if (e.Data.GetDataPresent (DataFormats.Html))
                DropHTMLText ((string)e.Data.GetData (DataFormats.Html));
        }

        private void MainDlg_MouseDown (object sender, MouseEventArgs e)
        {
            DataObject dataObject;

            // make certain that a valid link is in the German fields
            if (string.IsNullOrEmpty (txtDeURL.Text))
                return;

            string linkHtml = BuildLinkTag (txtDeText.Text, txtDeURL.Text);
            string env = ClipboardEnvelope.BuildHtmlClipboardEnvelope (linkHtml);
            dataObject = new DataObject ();
            dataObject.SetData ("HTML Format", env);

            // start a Drag&Drop process
            _emitingDragDrop = true;
            DragDropEffects effs = txtDeURL.DoDragDrop (dataObject, DragDropEffects.Copy);
            _emitingDragDrop = false;
        }


        /**********************************************************************
        *** Processing Functions
        **********************************************************************/

        private void ProcessFile (string filePath)
        {
            _filePath = filePath;
            UpdateDialogTitle ();
            Cursor.Current = Cursors.WaitCursor;

            // read the OpenOffice document into memory; if the user has the same file still open
            // in OpenOffice this operation fails with an exception; we then giv ehim the chance to
            // close the document and retry.
            bool retry = false;
            _doc = new OpenOfficeDoc ();
            do
            {
                try
                {
                    stsAction.Text = "Reading file";
                    if (!_doc.Read (filePath))
                        return;
                    break;
                }
                catch (Exception ex)
                {
                    DialogResult res = MessageBox.Show ("Error: Could not read file from disk. Original error: " + ex.Message,
                        "File Open Error", MessageBoxButtons.RetryCancel);
                    if (res == DialogResult.Retry)
                        retry = true;
                    else
                    {
                        stsAction.Text = "Processing aborted.";
                        Cursor.Current = Cursors.Default;
                        return;
                    }
                }
            } while (retry);

            // extract the hyperlinks from the document
            stsAction.Text = "Extracting hyperlinks";
            ClearAllLinks ();
            if (!_doc.ExtractLinks(_enLinks))
            {
                Cursor.Current = Cursors.Default;
                return;
            }

            // translate these hyperlinks
            stsAction.Text = "Translating hyperlinks";
            _lt.TranslateLinks2 (_enLinks, _deLinks);

            // show  the links and their translations
            _curIdx = 0;
            ShowCurrentLink ();
            stsAction.Text = "Done";
            Cursor.Current = Cursors.Default;
            appendLinksToDocumentToolStripMenuItem.Enabled = true;
        }

        private void ClearAllLinks ()
        {
            _enLinks.Clear ();
            _deLinks.Clear ();
            _curIdx = -1;
            ShowCurrentLink ();
        }

        private void ShowCurrentLink ()
        {
            _isInitializing = true;

            if (_curIdx < 0)
            {
                lblNumber.Text = string.Empty;
                txtEnText.Text = string.Empty;
                txtEnURL.Text = string.Empty;
                txtDeText.Text = string.Empty;
                txtDeURL.Text = string.Empty;
                btnPrevious.Enabled = false;
                btnNext.Enabled = false;
                btnAddTextToDB.Enabled = false;
                btnAddURLtoDB.Enabled = false;
                lblTransMethodText.Text = string.Empty;
                lblTransMethodUrl.Text = string.Empty;
                return;
            }

            lblNumber.Text = string.Format ("{0} of {1}", _curIdx + 1, _deLinks.Count);
            txtEnText.Text = _enLinks[_curIdx].text;
            txtEnURL.Text = _enLinks[_curIdx].uri;
            txtDeText.Text = _deLinks[_curIdx].text;
            txtDeURL.Text = _deLinks[_curIdx].uri;
            lblTransMethodText.Text = _deLinks[_curIdx].transMethodText;
            lblTransMethodText.ForeColor = (_deLinks[_curIdx].transMethodText == "none") ? Color.Red :
                 (_deLinks[_curIdx].transMethodText != "DB") ? Color.Orange : 
                 Color.Green;
            lblTransMethodUrl.Text = _deLinks[_curIdx].transMethodUrl;
            lblTransMethodUrl.ForeColor = (_deLinks[_curIdx].transMethodUrl == "none") ?
                Color.Red : Color.Green;
            btnPrevious.Enabled = _curIdx > 0;
            btnNext.Enabled = _curIdx < _enLinks.Count - 1;

            btnAddTextToDB.Enabled = _deLinks[_curIdx].textChanged && !_readOnlyMode;
            btnAddURLtoDB.Enabled = _deLinks[_curIdx].uriChanged && !_readOnlyMode;

            _isInitializing = false;
        }

        private void UpdateDialogTitle ()
        {
            string title = "Link Translator";
            if (!string.IsNullOrEmpty (_filePath))
                title += " - " + _filePath;
            this.Text = title;
        }

        private void DropFiles (string[] filePaths)
        {
            foreach (string filePath in filePaths)
            {
                ProcessFile (filePath);
                return; // only process the first fils
            }
        }

        private void DropHTMLText (string htmlString)
        {
            ClearAllLinks ();
            _curIdx = -1; // in case we don't find any links

            // set wait cursor
            Cursor.Current = Cursors.WaitCursor;

            // unpack the clipboard envelope
            ClipboardEnvelope env = new ClipboardEnvelope ();
            if (!env.Read (htmlString))
                return;
            string fragment = env.GetFragmentString ();

            // parse the HTML fragment and extract the <A href=...> element
            HtmlMiniParser parser = new HtmlMiniParser (fragment);
            while (parser.ReadTag ())
            {
                if (parser.TagName.ToLower () == "a")
                {
                    string url = "";
                    while (parser.ReadAttribute ())
                    {
                        if (parser.AttributeName.ToLower () == "href")
                        {
                            url = parser.AttributeValue;
                            break;
                        }
                    }
                    StringBuilder sb = new StringBuilder ();
                    sb.Append (parser.IntraTagText);

                    // consume all intermediate tags until we see the </a>
                    while (parser.ReadTag ())
                    {
                        if (parser.TagName.ToLower () == "/a")
                            break;
                        sb.Append (parser.IntraTagText);
                    }

                    if (url.Length > 0)
                    {
                        string text = System.Web.HttpUtility.HtmlDecode (sb.ToString ());
                        Hyperlink hl = new Hyperlink();
                        hl.uri = url;
                        hl.text = text;

                        // enter the link into the link list
                        _enLinks.Add (hl);
                    }
                }
            }

            // tranlate the links
            _lt.TranslateLinks2 (_enLinks, _deLinks);

            // show the link and its translations
            _curIdx = _enLinks.Count > 0 ? 0 : -1;
            ShowCurrentLink ();

            // cursor back to normal
            Cursor.Current = Cursors.Default;
        }


        /// <summary>
        /// Returns most recently written File from the specified directory.
        /// If the directory does not exist or doesn't contain any file, null is returned.
        /// </summary>
        /// <param name="directoryInfo">Path of the directory that needs to be scanned</param>
        private static string NewestFileofDirectory (string directoryPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo (directoryPath);
            if (directoryInfo == null || !directoryInfo.Exists)
                return null;

            FileInfo[] files = directoryInfo.GetFiles ();
            DateTime recentWrite = DateTime.MinValue;
            FileInfo recentFile = null;

            foreach (FileInfo file in files)
            {
                if (file.LastWriteTime > recentWrite)
                {
                    recentWrite = file.LastWriteTime;
                    recentFile = file;
                }
            }
            return recentFile.Name;
        }

        private static string BuildLinkTag (string text, string url)
        {
            // the text part needs to be escaped for HTML
            string encodedText = System.Web.HttpUtility.HtmlEncode (text);
            
            StringBuilder sb = new StringBuilder ();
            sb.Append ("<A HREF=\"");
            sb.Append (url);
            sb.Append ("\">");
            sb.Append (encodedText);
            sb.Append ("</A>");
            return sb.ToString ();
        }

        private void CheckAppFolderAccess()
        {
            _readOnlyMode = true;

            // replace empty path by the current directory, so we can test the existence of it
            string dataFolder = Helpers.AppDataFolder;
            if (string.IsNullOrEmpty(dataFolder))
                dataFolder = Directory.GetCurrentDirectory();

            // check that directory exists
            if (!Directory.Exists(dataFolder))
            {
                MessageBox.Show($"The translation database folder {Properties.Settings.Default.appDataFolderPath} does not exist.",
                    "Directory not found", MessageBoxButtons.OK);
                return;
            }

            // check write access on that directory
            if (!Helpers.CheckDirectoryWriteAccess(dataFolder))
            {
                if (string.IsNullOrEmpty(dataFolder))
                    dataFolder = Directory.GetCurrentDirectory();
                MessageBox.Show($"The folder {dataFolder} cannot be written to by this application. " +
                    "Go to Tools->Configuration... to switch to a writable folder and place the " +
                    "translation files TransUrlDb.txt and TransTextDb.txt in there!\n\n" +
                    "Until you do that, you will not be able to add new entries to the translation database.",
                    "No write access to directory", MessageBoxButtons.OK);
                return;
            }

            // we do have write access
            _readOnlyMode = false;
        }
    }
}