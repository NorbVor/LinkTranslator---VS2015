using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace LinkTranslator
{
    /// <summary>
    /// This class contains the actual core functionality of the program: Translating
    /// hyperlinks from English to German. Its TranslateLinks function takes a list
    /// of English Hyperlinks and translates them - if possible - to their German
    /// equivalents. We only translate links to Wikipedia.org at this time.
    /// 
    /// The translation process is based on three methods:
    /// a) Translation via a precollected XML-Database
    /// b) Using the Wikidata service
    /// c) Decoding the Wikipedia page itself and finding the inter-language links in
    ///    its HTML.
    /// </summary>
    class LinkTranslator
    {
        // in-memory representation of the XML-Database of link translations
        private Dictionary<string, TransElem> _topicDict;

        /// <summary>
        /// As part of the constructor, we read the XML-file with our translation database
        /// and build the in-memory data structure for it.
        /// </summary>
        public LinkTranslator ()
        {
            _topicDict = new Dictionary<string, TransElem>();
            ReadLinkDatabaseXml ("LinkDatabase.xml");
        }

        /// <summary>
        /// Loops over all links in the input list and tries to translate them. The translated
        /// links are returned in the output list. Untranslated links are just copied to the
        /// output list.
        /// 
        /// We categorize hyperlinks into three groups:
        /// a) Links to ESO pages -- they don't need translation, as the ESO website contains
        ///    translations of its pages and lets the user choose its local language.
        /// 
        /// b) Links to Wikipedia pages. Those are the ones we want to translate
        /// 
        /// c) Any other links. They usually cannot be translated and we leave them alone.
        /// </summary>
        /// <param name="links">Input list</param>
        /// <param name="transLinks">Output List</param>
        public void TranslateLinks (List<Hyperlink> links, List<Hyperlink> transLinks)
        {
            foreach (Hyperlink hl in links)
            {
                Hyperlink transHL;

                Uri uri = new Uri (hl.uri);
                if (uri.Authority == "www.eso.org")
                {
                    // ESO links don't need to be translated
                    transHL = (Hyperlink) hl.Clone ();
                    transHL.transStatus = Hyperlink.TranslationStatus.eso; 
                }
                else if (uri.Authority == "en.wikipedia.org")
                    transHL = TranslateWikipedialLink (hl);
                else
                {
                    // all other links can't be translated yet
                    transHL = (Hyperlink) hl.Clone ();
                    transHL.transStatus = Hyperlink.TranslationStatus.notrans;
                }

                transLinks.Add (transHL);
            }
        }

        /// <summary>
        /// This is the core function of the class. It tries to translate a hyperlink
        /// to an English Wikipedia page into its German equivalent.
        /// </summary>
        /// <param name="hl"></param>
        /// <returns>A Hyperlink object with either the translated, partially translated
        /// or untranslated link.</returns>
        public Hyperlink TranslateWikipedialLink (Hyperlink hl)
        {
            // Create a new Hyperlink object and clone the contents from our source object,
            // just in case none of our translation methods works.
            Hyperlink deHL = (Hyperlink)hl.Clone ();
            deHL.transStatus = Hyperlink.TranslationStatus.notrans;

            // Try to translate the hyperlink in a three-step cascade:
            // a) via XML-Database
            // b) via Wikidata
            // c) vie the Wikipedia page itself
            if (!TranslateByDatabaseXML (hl, deHL))
                if (!FindGermanWikiTopicByWikidata (hl, deHL))
                    if (!FindGermanWikiTopicByPageScan (hl, deHL))
                        // In any case we return a new Hyperlink object. If no translation could
                        // be performed the transStatus member will show that.
                        return deHL;

            // we were successful!
            return deHL;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /// Data Structures of the XML-Database in-memory representation
        /// 

        /*
         * The dictionary contains TransElem objects which focus on the translation of the URLs and
         * tell the German equivalent to an English URL. In addition it offers several text translations
         * that have been seen with that URL in a list of TransPair elements.
         */

        private class TransElem
        {
            public TransElem ()
            {
                textPairs = new List<TransPair> ();
            }
            public string enUri;
            public string deUri;
            public List<TransPair> textPairs;
        }

        private class TransPair
        {
            public string en, de;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /// private functions
        /// 
        private void ReadLinkDatabaseXml (string filePath)
        {
            XmlDocument xml = new XmlDocument ();
            xml.Load (filePath);
            XmlNode root = xml.DocumentElement;

            foreach (XmlNode node in root.ChildNodes)
            {
                int countChildern = node.ChildNodes.Count;
                
                TransElem te = new TransElem ();
                te.enUri = node["enUri"].InnerText;
                te.deUri = node["deUri"].InnerText;

                foreach (XmlNode textPairNode in node.ChildNodes)
                {
                    // skip the two uri nodes
                    if (textPairNode.Name != "Trans")
                        continue;

                    // every textPairNode is expected to have exactly
                    // two children en and de
                    TransPair tp = new TransPair();
                    tp.en = NormalizeText (textPairNode["en"].InnerText);
                    tp.de = textPairNode["de"].InnerText;
                    te.textPairs.Add (tp);
                }

                _topicDict.Add (ToWikiTopic(te.enUri), te);
            }
        }

        private bool TranslateByDatabaseXML (Hyperlink hl, Hyperlink deHL)
        {
            TransElem entry;

            // extract topic
            string topic = ToWikiTopic (hl.uri);
            string normalizedText = NormalizeText (hl.text);

            if (!_topicDict.TryGetValue (topic, out entry))
                return false;

            deHL.uri = entry.deUri;
            deHL.transStatus = Hyperlink.TranslationStatus.partial;

            // there may be several text links to the same URI; pick the correct entry
            // to translate the text
            foreach (TransPair tp in entry.textPairs)
            {
                if (tp.en == normalizedText)
                {
                    deHL.text = tp.de;
                    deHL.transStatus = Hyperlink.TranslationStatus.full;
                    break;
                }
            }

            // if the translated URI is in the English Wiki, we set the
            // status back to partial
            if (deHL.uri.Contains ("//en."))
            {
                deHL.transStatus = Hyperlink.TranslationStatus.partial;
            }

            deHL.transMethod = "db";
            return true;
        }

        private bool FindGermanWikiTopicByWikidata (Hyperlink hl, Hyperlink deHL)
        {
            string topic = ToWikiPageName (hl.uri);
            if (topic == null || topic == "")
                return false;

            string request = "https://www.wikidata.org/w/api.php?action=wbgetentities" +
                "&sites=enwiki&languages=en|de&titles=" + topic +
                "&props=sitelinks/urls&format=xml&sitefilter=enwiki|dewiki";

            XmlReader xmlReader = XmlReader.Create (request);

            while (xmlReader.Read ())
            {
                if (xmlReader.NodeType == XmlNodeType.Element &&
                    xmlReader.Name == "sitelink" &&
                    xmlReader.GetAttribute ("site") == "dewiki")
                {
                    deHL.uri = xmlReader.GetAttribute ("url");
                    deHL.text = xmlReader.GetAttribute ("title");
                    deHL.transStatus = Hyperlink.TranslationStatus.partial;
                    deHL.transMethod = "wikidata";
                    return true;
                }
            }

            return false;
        }

        private bool FindGermanWikiTopicByPageScan (Hyperlink hl, Hyperlink deHL)
        {
            XmlReaderSettings settings = new XmlReaderSettings ();
            settings.ProhibitDtd = false;
            XmlReader xmlReader = XmlReader.Create (hl.uri, settings);

            bool langLabelSeen = false;
            while (xmlReader.Read ())
            {
                if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "h3"))
                {
                    if (xmlReader.GetAttribute ("id") == "p-lang-label")
                        langLabelSeen = true;
                }

                if (langLabelSeen && (xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "a"))
                {
                    if (xmlReader.GetAttribute ("lang") == "de")
                    {
                        deHL.uri = xmlReader.GetAttribute ("href");
                        deHL.transStatus = Hyperlink.TranslationStatus.partial;

                        deHL.text = hl.text; // in case we don't find a title
                        string title = xmlReader.GetAttribute ("title");
                        if (title != null)
                        {
                            char dash = title[title.Length - 8];
                            int idx = title.LastIndexOf (" \u2013 German");
                            if (idx > 0)
                                title = title.Substring (0, idx);
                            deHL.text = title;
                        }
                        deHL.transMethod = "wiki page";
                        return true;
                    }
                }
            }
            return false;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /// helper functions
        /// 

        /// <summary>
        /// Extract the Wikipedia topic name from a complete URL String.
        /// Example:
        ///   "https://en.wikipedia.org/wiki/Barnard%27s_Loop" -> "Barnard's_Loop"
        /// 
        /// There are two differences to function ToWIkiPageName:
        /// 1) If the URL does not contain the "/wiki/" component, it will instead
        ///    use the part following the last slash "/" character. (no longer necessary)
        /// 2) Any %-escape sequences in the URL string are being converted.
        /// </summary>
        private string ToWikiTopic (string uriString)
        {
            int idx = uriString.IndexOf ("/wiki/");
            if (idx >= 0)
            {
                idx += 6; // length of /wiki/
            }
            else
            {
                idx = uriString.LastIndexOf ('/');
                if (idx >= 0)
                    idx += 1;
                else
                    idx = 0; // take whole string
            }
            string topic = uriString.Substring (idx);

            // remove %xx escapes
            topic = Uri.UnescapeDataString (topic);
            return topic;
        }

        /// <summary>
        /// Extract the Wikipedia page name from a complete URL string
        /// Example:
        ///   "https://en.wikipedia.org/wiki/Milky_Way" -> "Milky_Way"
        /// Note that the result is case sensitive!
        /// </summary>
        private string ToWikiPageName (string uriString)
        {
            Uri uri = new Uri (uriString);
            string topic = uri.PathAndQuery;

            if (topic.StartsWith ("/wiki/"))
                topic = topic.Substring (6);
            return topic;
        }

        /// <summary>
        /// Normalize a hyperlink text string to enhance finding an equivalent in our
        /// translation database. Leading and trailing spaces are removed as well
        /// as all commas, periods and colons. And the string is translated to all
        /// lower case.
        /// </summary>
        private string NormalizeText (string s)
        {
            s = s.Trim ();
            s.Replace (",", "");
            s.Replace (".", "");
            s.Replace (":", "");
            s = s.ToLower ();
            return s;
        }


#if false
        // This function was previously used to convert the CSV-format translation file
        // into the XML-based translation database. Now obsolete.
        private void ConvertLinkDatabaseToXML (string filePathIn, string filePathOut)
        {
            XmlDocument xml = new XmlDocument ();
            XmlElement root = xml.CreateElement ("HyperlinkTranslationDb");
            xml.AppendChild (root);

            using (StreamReader reader = new StreamReader (filePathIn))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine ();
                    string[] items = line.Split (';');
                    if (items.Length < 4)
                        continue; // illegal line

                    XmlElement trans = (XmlElement) FindNodeForEnUri (root, items[1]);
                    if (trans == null)
                    {
                        trans = xml.CreateElement ("TransElem");
                        XmlElement fromUri = xml.CreateElement ("enUri");
                        trans.AppendChild (fromUri);
                        fromUri.InnerText = items[1];
                        XmlElement toUri = xml.CreateElement ("deUri");
                        toUri.InnerText = items[3];
                        trans.AppendChild (toUri);

                        root.AppendChild (trans);
                    }
                    else
                    {
                        // check that the node has the same deUri translateion
                        if (trans["deUri"].InnerText != items[3])
                            continue; // error
                    }

                    // add nodes for the text translation
                    XmlElement tp = xml.CreateElement ("Trans");
                    trans.AppendChild (tp);
                    XmlElement enText = xml.CreateElement ("en");
                    enText.InnerText = items[0];
                    XmlElement deText = xml.CreateElement ("de");
                    deText.InnerText = items[2];
                    tp.AppendChild (enText);
                    tp.AppendChild (deText);
                }
            }

            xml.Save (filePathOut);
        }

        /// <summary>
        /// Find the node for a given URI in XML tree.
        /// </summary>
        private XmlNode FindNodeForEnUri (XmlElement root, string uri)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                if (node["enUri"].InnerText == uri)
                    return node;
            }
            return null;
        }
#endif

    }
}
