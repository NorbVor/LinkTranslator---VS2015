using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;
using Ionic.Zip;
using System.Windows.Forms;

namespace LinkTranslator
{
    /// <summary>
    /// The OpenOfficeDoc class is wrapper around all the functionality needed to read and
    /// write OpenOffice documents.
    /// </summary>
    class OpenOfficeDoc
    {
        // the file path we have been reading from
        private string _filepath;

        // the contents of content.xml as .NET XmlDocument
        private XmlDocument _doc;

        // namespace URIs
        private string _nsText;
        private string _nsOffice;
        private string _nsXlink;

        // flag that a line of dashes has been found
        private bool _docContainsLangSeparator = false;

        /// <summary>
        /// Opens the OpenOffice document by unzipping it and extracting the content.xml
        /// file from the zip archive. This file is read into a MemoryStream and converted
        /// from UTF-8 to a String object. This string is then parsed into XML tree that
        /// other member functions act upon.
        /// </summary>
        /// <param name="filePath">File path of OpenOffice document</param>
        /// <returns>True upon success</returns>
        public bool Read (string filePath)
        {
            bool retry = false;
            do
            {
                try
                {
                    retry = false;
                    _filepath = filePath;
                    using (ZipFile zip = ZipFile.Read(_filepath))
                    {
                        // read the content.xml file inside the zip archive into _memStream
                        ZipEntry contentZip = zip["content.xml"];
                        MemoryStream memStream = new MemoryStream(50000);
                        contentZip.Extract(memStream);

                        // convert _memString into a string
                        memStream.Seek(0, SeekOrigin.Begin);
                        int count;
                        byte[] byteArray;
                        byteArray = new byte[memStream.Length];
                        count = memStream.Read(byteArray, 0, (int)memStream.Length);
                        String s = Encoding.UTF8.GetString(byteArray);

                        // load that string into an XmlDocument object
                        _doc = new XmlDocument();
                        _doc.LoadXml(s);
                    }
                }
                catch
                {
                    DialogResult res = MessageBox.Show($"Error: File {filePath} could not be opened",
                        "File Open Error", MessageBoxButtons.RetryCancel);
                    if (res == DialogResult.Retry)
                        retry = true;
                    else
                        return false;
                }
            } while (retry);
            return true;
        }

        /// <summary>
        /// Extract all hyperlinks from an OpenOffice document. The document must have
        /// been read by the Read member function.
        /// </summary>
        /// <param name="links">The list into which the Hyperlink are being placed</param>
        /// <returns>True on success</returns>
        public bool ExtractLinks (List<Hyperlink> links)
        {
            XmlNode root = _doc.DocumentElement;
            XmlNode bodyNode = root["office:body"];

            return WalkDocTree (bodyNode, links);
        }

        /// <summary>
        /// Returns true when a line of dashes has been found in the course of scanning
        /// the document. The information is only valid after ExtractLinks has been called
        /// on the document.
        /// 
        /// Note: We use this information when collecting existing translations in a
        /// directory of already translated pages.
        /// </summary>
        public bool ContainsLangSeparator ()
        {
            return _docContainsLangSeparator;
        }

        /// <summary>
        /// Appends a list of Hyperlinks to an open OpenOffice document that has been
        /// read by the Read member function. The hyperlinks are added by attaching 
        /// appropriate content to the document tree. The document tree is not saved
        /// to disk. This has to be done by a call to the Save member function.
        /// </summary>
        /// <param name="links"></param>
        /// <returns>True on success.</returns>
        public bool AppendLinksToDoc (List<Hyperlink> links)
        {
            // extract the namespace URIs used to generate text and hyperlink nodes
            XmlNode root = _doc.DocumentElement;
            XmlAttribute xmlnsAttrOffice = root.Attributes["xmlns:office"];
            XmlAttribute xmlnsAttrText = root.Attributes["xmlns:text"];
            XmlAttribute xmlnsAttrXlink = root.Attributes["xmlns:xlink"];
            if (xmlnsAttrOffice == null || xmlnsAttrText == null || xmlnsAttrXlink == null)
                return false;
            _nsOffice = xmlnsAttrOffice.InnerText;
            _nsText = xmlnsAttrText.InnerText;
            _nsXlink = xmlnsAttrXlink.InnerText;

            // retrieve the office:text node of the document
            XmlNode bodyNode = root["office:body"];
            if (bodyNode == null)
                return false;
            XmlNode textNode = bodyNode["office:text"];
            if (textNode == null)
                return false;

            // now we append all generated content to that textNode
            AddSeparatorLine (textNode);
            XmlNode parWithLinks = AddTextParagraph (textNode);
            AddLinks (parWithLinks, links);

            return true;
        }

        /// <summary>
        /// Saves the current document tree to disk file. The original OpenOffice file
        /// as opened by the Read member function is being read again and the file content.xml
        /// is being replaced.
        /// </summary>
        /// <param name="filePath">File path of the file that is going to be created.</param>
        /// <returns>True on success</returns>
        public bool Save (string filePath)
        {
            MemoryStream memStream = new MemoryStream (10000);
            _doc.Save (memStream);
            memStream.Seek (0, SeekOrigin.Begin);

            // XmlDocument.Save writes a BOM (byte order mark) to the begin of the stream.
            // But XmlDocument.LoadXml(string) doesn't like that same BOM and throws an exception
            // when seen. So we skip it, if we find one.
            byte[] a;
            a = new byte[10];
            memStream.Read (a, 0, 10);
            if (a[0] == 0xef && a[1] == 0xbb && a[2] == 0xbf)
                memStream.Seek (3, SeekOrigin.Begin);

            // replace the memStream in the original zip file
            using (ZipFile zip = ZipFile.Read (_filepath))
            {
                zip.UpdateEntry ("content.xml", memStream);
                zip.Save (filePath);
            }
            return true;
        }

        /************************************************************************************
        ***     Private Functions
        ************************************************************************************/

        /// <summary>
        /// Recursively traverses the entire document tree and looks for any "a"-Nodes that
        /// contain hyperlinks. It extracts the href attribute and inner text from the node
        /// and creates a Hyperlink object from them, which is then adder to the links list.
        /// 
        /// While traversing the document tree this function also looks for a paragraph starting
        /// with five dashes and sets the member variable _docContainsLangSeparator when found.
        /// This indication tells us that the document contains already translated text as
        /// by our own convention.
        /// </summary>
        /// <param name="node">The node from which to start the tree traversal</param>
        /// <param name="links">The list to which to add any created Hyperlink objects</param>
        /// <returns></returns>
        private bool WalkDocTree (XmlNode node, List<Hyperlink> links)
        {
            IEnumerator ienum = node.GetEnumerator ();
            XmlNode childNode;
            while (ienum.MoveNext ())
            {
                childNode = (XmlNode)ienum.Current;
                if (childNode.LocalName == "p" && childNode.InnerText.StartsWith ("-----"))
                {
                    _docContainsLangSeparator = true;
                }
                if (childNode.LocalName == "a")
                {
                    XmlAttribute attr = childNode.Attributes["xlink:href"];
                    if (attr == null)
                        return false;
                    if (childNode.InnerText == null || childNode.InnerText.Length == 0)
                        return false;

                    // add that hyperlink to the list
                    Hyperlink link = new Hyperlink ();
                    link.text = childNode.InnerText;
                    link.uri = attr.InnerText;
                    links.Add (link);
                }
                else
                    if (!WalkDocTree (childNode, links))
                        return false;
            }
            return true;
        }

        /*
         * The following four helper functions are used to add contents to the document tree. We use
         * them in AppendLinksToDoc to append the translated links at the end of the current document.
         */
        private void AddSeparatorLine (XmlNode node)
        {
            XmlNode newParagraph = _doc.CreateElement ("text:p", _nsText);
            newParagraph.InnerText = "----------------------------------------------------------------";
            node.AppendChild (newParagraph);
        }

        private XmlNode AddTextParagraph (XmlNode node)
        {
            XmlNode newParagraph = _doc.CreateElement ("text:p", _nsText);
            return node.AppendChild (newParagraph);
        }

        private void AddLinks (XmlNode parNode, List<Hyperlink> links)
        {
            foreach (Hyperlink hl in links)
            {
                parNode.InnerXml += hl.StatusMsg + "  ";

                XmlNode newLink = _doc.CreateElement ("text:a", _nsText);
                newLink.InnerText = hl.text;
                XmlAttribute newAttr = _doc.CreateAttribute ("xlink:href", _nsXlink);
                newAttr.InnerText = hl.uri;
                newLink.Attributes.Append (newAttr);
                parNode.AppendChild (newLink);

                // separate links by a line break
                AddLineBreak (parNode);
            }
        }

        private void AddLineBreak (XmlNode parNode)
        {
            XmlNode breakTag = _doc.CreateElement ("text:line-break", _nsText);
            parNode.AppendChild (breakTag);
        }

    }
}
