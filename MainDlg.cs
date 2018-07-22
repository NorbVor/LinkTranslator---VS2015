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

namespace LinkTranslator
{
    public partial class MainDlg : Form
    {
        private string _filePath;
        private List<Hyperlink> _enLinks;
        private List<Hyperlink> _deLinks;
        private int _curIdx = -1;
        OpenOfficeDoc _doc;
        bool _emitingDragDrop = false;

        public MainDlg ()
        {
            InitializeComponent ();
            lblNumber.Text = "";

            _enLinks = new List<Hyperlink>();
            _deLinks = new List<Hyperlink>();
            _curIdx = -1;
            ShowCurrentLink ();
            btnAppendToDoc.Enabled = false;
            stsAction.Text = "";
        }

        /**********************************************************************
        *** Event Handlers
        **********************************************************************/

        private void btnTransLinks_Click (object sender, EventArgs e)
        {
            string documentFolderPath = Properties.Settings.Default.documentFolderPath;

            // find the newest file in the document directory in order to
            // preselect it in the file open dialog
            string preselFileName = NewestFileofDirectory (documentFolderPath);

#if DEBUG
            documentFolderPath = "";
            preselFileName = "OpenOfficeDoc.odt";
#endif

            // open a file dialog to ask for the file to work on
            OpenFileDialog dlg = new OpenFileDialog ();
            dlg.InitialDirectory = documentFolderPath;
            dlg.Filter = "OpenOffice files (*.odt)|*.odt|All files (*.*)|*.*";
            if (preselFileName != null)
                dlg.FileName = preselFileName;
            dlg.RestoreDirectory = true;
            if (dlg.ShowDialog () != DialogResult.OK)
                return;

            ProcessFile (dlg.FileName);
        }


        // append the translated hypelinks at the end of the document
        private void btnAppendToDoc_Click (object sender, EventArgs e)
        {
            if (_doc == null || _filePath == null)
                return;
            stsAction.Text = "Appending hyperlinks to document";
            if (!_doc.AppendLinksToDoc (_deLinks))
                return;

            // save the document back to disk
            string outputPath = _filePath;
            stsAction.Text = "Saving file: " + _filePath;
#if DEBUG
            outputPath = "newDoc.odt";
#endif
            _doc.Save (outputPath);
            stsAction.Text = "Done";

            _doc = null;
            _filePath = null;
            ClearAllLinks ();
            UpdateDialogTitle ();
            btnAppendToDoc.Enabled = false;
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
            if (_curIdx < 0)
                return;
            _deLinks[_curIdx].text = txtDeText.Text;
        }

        private void txtDeURL_TextChanged (object sender, EventArgs e)
        {
            if (_curIdx < 0)
                return;
            _deLinks[_curIdx].uri = txtDeURL.Text;
        }

        /*
         * Tool Strip Event Handlers
         */
        private void configureToolStripMenuItem_Click (object sender, EventArgs e)
        {
            // open the configuration dialog
            ConfigDlg dlg = new ConfigDlg ();
            dlg.ShowDialog ();
        }

        private void collectLinksToolStripMenuItem_Click (object sender, EventArgs e)
        {
            LinkCollector linkCol = new LinkCollector ();
            linkCol.Execute ();
        }

        private void aboutToolStripMenuItem_Click (object sender, EventArgs e)
        {
            // show About dialog
            AboutBox dlg = new AboutBox ();
            dlg.ShowDialog (this);
        }

        private void showHelpToolStripMenuItem_Click (object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start ("LinkTranslatorHelp.html");
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
                        return;
                    }
                }
            } while (retry);

            // extract the hyperlinks from the document
            stsAction.Text = "Extracting hyperlinks";
            ClearAllLinks ();
            if (!_doc.ExtractLinks (_enLinks))
                return;

            // translate these hyperlinks
            stsAction.Text = "Translating hyperlinks";
            List<Hyperlink> transLinks = new List<Hyperlink> ();
            LinkTranslator tl = new LinkTranslator ();
            tl.TranslateLinks (_enLinks, _deLinks);

            // show  the links and their translations
            _curIdx = 0;
            ShowCurrentLink ();
            stsAction.Text = "Done";
            btnAppendToDoc.Enabled = true;
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
            if (_curIdx < 0)
            {
                lblNumber.Text = string.Empty;
                txtEnText.Text = string.Empty;
                txtEnURL.Text = string.Empty;
                txtDeText.Text = string.Empty;
                txtDeURL.Text = string.Empty;
                btnPrevious.Enabled = false;
                btnNext.Enabled = false;
                return;
            }

            lblNumber.Text = string.Format ("{0} of {1}", _curIdx + 1, _deLinks.Count);
            txtEnText.Text = _enLinks[_curIdx].text;
            txtEnURL.Text = _enLinks[_curIdx].uri;
            txtDeText.Text = _deLinks[_curIdx].text;
            txtDeURL.Text = _deLinks[_curIdx].uri;
            lblTransMethod.Text = _deLinks[_curIdx].transMethod;
            if (_deLinks[_curIdx].transMethod == "db")
                lblTransMethod.Text += " (" + _deLinks[_curIdx].transStatus.ToString () + ")";
            btnPrevious.Enabled = _curIdx > 0;
            btnNext.Enabled = _curIdx < _enLinks.Count - 1;
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
                    Hyperlink hl = new Hyperlink ();
                    while (parser.ReadAttribute ())
                    {
                        if (parser.AttributeName.ToLower () == "href")
                            hl.uri = parser.AttributeValue;
                        break;
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

                    string text = System.Web.HttpUtility.HtmlDecode (sb.ToString ());
                    hl.text = text;

                    // enter the link into the link list
                    _enLinks.Add (hl);
                }
            }

            // tranlate the links
            LinkTranslator tl = new LinkTranslator ();
            tl.TranslateLinks (_enLinks, _deLinks);

            // show the link and its translations
            _curIdx = _enLinks.Count > 0 ? 0 : -1;
            ShowCurrentLink ();
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
    }
}