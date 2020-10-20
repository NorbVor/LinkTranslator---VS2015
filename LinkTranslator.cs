using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using LinkTranslator.Properties;
using System.Net;

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
#if false
        // in-memory representation of the XML-Database of link translations
        private Dictionary<string, TransElem> _topicDict;
#endif
        // new dictornary structure:
        private Dictionary<string, string> _urlDict;
        private Dictionary<string, string> _textDict;
        private const string _fileNameUrlDB = "TransUrlDb.txt";
        private const string _fileNameTextDB = "TransTextDb.txt";

        /// <summary>
        /// As part of the constructor, we read the XML-file with our translation database
        /// and build the in-memory data structure for it.
        /// </summary>
        public LinkTranslator ()
        {
#if false
            _topicDict = new Dictionary<string, TransElem>();
            ReadLinkDatabaseXml ("LinkDatabase.xml");
#endif
            // new dictonary: load the dictionaries
            ReloadDatabase();

            // preparation for the new database that uses two text files for links and text representation
            //GenLinkUrlTransTable("LinkDb.txt");
            //GenLinkTextTransTable("TextDb.txt");
        }

        public void ReloadDatabase ()
        {
            if (!LoadUrlDatabase(_fileNameUrlDB))
                MessageBox.Show($"Could not find the URL translation database file {_fileNameUrlDB}", "File not found", MessageBoxButtons.OK);
            if (!LoadTextDatabase(_fileNameTextDB))
                MessageBox.Show($"Could not find the text translation database file {_fileNameTextDB}", "File not found", MessageBoxButtons.OK);
        }
#if false
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
                else if (uri.Authority == "en.wikipedia.org" || uri.Authority == "wikipedia.org")
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
#endif

        public void TranslateLinks2(List<Hyperlink> links, List<Hyperlink> transLinks)
        {
            foreach (Hyperlink hl in links)
                transLinks.Add(TranslateLink2(hl));
        }

        public void ReTranslateESOLinks(List<Hyperlink> links, List<Hyperlink> transLinks)
        {
            for (int idx = 0; idx < links.Count; ++idx)
            {
                if (transLinks[idx].transMethodUrl == "ESO")
                     TranslateESOLink(links[idx], transLinks[idx]);
            }
        }

        public Hyperlink TranslateLink2(Hyperlink hl)
        {
            // the default is to use the original URL and text
            Hyperlink transHL;
            transHL = (Hyperlink)hl.Clone();
            transHL.transMethodUrl = "none";
            transHL.transMethodText = "none";

            string authority = GetUrlAuthority(hl.uri);
            if (authority == "www.eso.org" || authority == "eso.org")
            {
                TranslateESOLink(hl, transHL);
            }
            else
            {
                bool isWikipediaLink = false;

                // try translate by the URI / text databases
                bool foundUri = TranslateUriByDatabase2(hl.uri, transHL, out isWikipediaLink);
                bool foundText = TranslateTextByDatabase2(hl.text, transHL);

                // for Wikipedia links we have some more means to find the link equivalent
                if (isWikipediaLink && (!foundUri || !foundText))
                {
                    if (!FindGermanWikiTopicByWikidata(hl, transHL, foundUri, foundText))
                        FindGermanWikiTopicByPageScan(hl, transHL, foundUri, foundText);
                }
            }
            return transHL;
        }

        private string GetUrlAuthority (string url)
        {
            string urlAuthority = "";
            try
            {
                Uri uri = new Uri(url);
                urlAuthority = uri.Authority;
            }
            catch
            {
            }
            return urlAuthority;
        }

        private void TranslateESOLink (Hyperlink hl, Hyperlink transHL)
        {
            transHL.transStatus = Hyperlink.TranslationStatus.eso;
            transHL.transMethodUrl = "ESO";

            // ESO URLs don't need to be translated
            // but the text part must still be translated
            transHL.uri = hl.uri;
            TranslateTextByDatabase2(hl.text, transHL);

            // as of Jan 2019: convert the link to an internal link by deleting
            // the https://www.eso.org part
            if (Settings.Default.reduceESOLinks)
            {
                transHL.uri = transHL.uri.Replace("https://www.eso.org", "");
                transHL.uri = transHL.uri.Replace("http://www.eso.org", "");
                transHL.uri = transHL.uri.Replace("http://eso.org", "");
            }
        }

        private bool TranslateUriByDatabase2 (string uri, Hyperlink transHL, out bool isWikipediaLink)
        {
            isWikipediaLink = false;

            string redUri = DeflateUrl(uri);
            if (redUri.Contains("$(enWiki)"))
                isWikipediaLink = true;

            string value;
            if (!_urlDict.TryGetValue(redUri, out value))
                return false;
            transHL.uri = InflateUrl(value);
            transHL.transStatus = Hyperlink.TranslationStatus.partial;
            transHL.transMethodUrl = "DB";
            return true;
        }

        private bool TranslateTextByDatabase2(string text, Hyperlink transHL)
        {
            // throw out the punctuation that is often part of a link and that makes it
            // unsuitable as a key. Also translate it to all lower-case.
            text = NormalizeText(text);

            string value;
            if (!_textDict.TryGetValue(text, out value))
                return false;
            transHL.text = value;
            if (transHL.transStatus == Hyperlink.TranslationStatus.notrans)
                transHL.transStatus = Hyperlink.TranslationStatus.partial;
            if (transHL.transStatus == Hyperlink.TranslationStatus.partial)
                transHL.transStatus = Hyperlink.TranslationStatus.full;
            transHL.transMethodText = "DB";
            return true;
        }

        private bool AddToUrlDict(string enUrl, string deUrl)
        {
            // abbreviate certain URLs
            enUrl = DeflateUrl(enUrl);
            deUrl = DeflateUrl(deUrl);

            // check whether we have that translation entry already
            string value;
            if (_urlDict.TryGetValue(enUrl, out value))
            {
                // we already have an entry for that key; see if the value is the same
                if (value == deUrl)
                    return false;

                // change entry to new text
                _urlDict[enUrl] = deUrl;
                return true;
            }


            // add the entry to the in-memory dictonary
            _urlDict.Add(enUrl, deUrl);
            return true;
        }

        private bool AddToTextDict (string enText, string deText)
        {
            enText = NormalizeText(enText);

            // check whether we have that translation entry already
            string value;
            if (_textDict.TryGetValue(enText, out value))
            {
                // we already have an entry for that key; see if the value is the same
                if (value == deText)
                    return false;

                // change entry to new text
                _textDict[enText] = deText;
                return true;
            }


            // add the entry to the in-memory dictonary
            _textDict.Add(enText, deText);
            return true;
        }

        public bool AddTextTranslation(string enText, string deText)
        {
            if (!AddToTextDict(enText, deText))
                return false;
            SaveTextDict();
            return true;
        }

        public bool AddUrlTranslation(string enUrl, string deUrl)
        {
            if (!AddToUrlDict(enUrl, deUrl))
                return false;
            SaveUrlDict();
            return true;
        }

#if false
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
#endif

        private bool LoadUrlDatabase (string fileName)
        {
            StreamReader inStream;
            string line;
            char[] separators = new char[] { ';' };
            _urlDict = new Dictionary<string, string>();
            string filePath = Path.Combine(Helpers.AppDataFolder, fileName);

            try
            {
                using (inStream = File.OpenText(filePath))
                {
                    while ((line = inStream.ReadLine()) != null)
                    {
                        string[] parts = line.Split(separators);
                        if (parts.Length < 2)
                            continue; // skip line
                        _urlDict.Add(UnescapeSemicolons(parts[0]), UnescapeSemicolons(parts[1]));
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool LoadTextDatabase (string fileName)
        {
            StreamReader inStream;
            string line;
            char[] separators = new char[] { ';' };
            _textDict = new Dictionary<string, string>();
            string filePath = Path.Combine(Helpers.AppDataFolder, fileName);

            try
            {
                using (inStream = File.OpenText(filePath))
                {
                    while ((line = inStream.ReadLine()) != null)
                    {
                        string[] parts = line.Split(separators);
                        if (parts.Length >= 2)
                            _textDict.Add(UnescapeSemicolons(parts[0]), UnescapeSemicolons(parts[1]));
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool SaveUrlDict()
        {
            string filePath = Path.Combine(Properties.Settings.Default.appDataFolderPath, _fileNameUrlDB);
            try
            {
                using (StreamWriter outStream = File.CreateText(filePath))
                {
                    // Acquire keys and sort them.
                    var keyList = _urlDict.Keys.ToList();
                    keyList.Sort();

                    foreach (var key in keyList)
                        outStream.WriteLine("{0};{1}", EscapeSemicolons(key), EscapeSemicolons(_urlDict[key]));
                }
                return true;
            }
            catch
            {
                MessageBox.Show($"Writing to URL translation database failed.", "File write error", MessageBoxButtons.OK);
                return false;
            }
        }

        private bool SaveTextDict()
        {
            string filePath = Path.Combine(Properties.Settings.Default.appDataFolderPath, _fileNameTextDB);
            try
            {
                using (StreamWriter outStream = File.CreateText(filePath))
                {
                    // Acquire keys and sort them.
                    var keyList = _textDict.Keys.ToList();
                    keyList.Sort();

                    foreach (var key in keyList)
                        outStream.WriteLine("{0};{1}", EscapeSemicolons(key), EscapeSemicolons(_textDict[key]));
                }
                return true;
            }
            catch
            {
                MessageBox.Show($"Writing to text translation database failed.", "File write error", MessageBoxButtons.OK);
                return false;
            }
        }

#if false
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

            deHL.transMethodUrl = "db";
            deHL.transMethodText = "db";
            return true;
        }
#endif

        private bool FindGermanWikiTopicByWikidata (Hyperlink hl, Hyperlink deHL, bool foundUrl = false, bool foundText = false)
        {
            string topic = ToWikiPageName (hl.uri);
            if (topic == null || topic == "")
                return false;

            XmlReaderSettings setgs = new XmlReaderSettings();
            setgs.DtdProcessing = DtdProcessing.Prohibit;
            setgs.MaxCharactersFromEntities = 1024;
            string request = "https://www.wikidata.org/w/api.php?action=wbgetentities" +
                "&sites=enwiki&languages=en|de&titles=" + topic +
                "&props=sitelinks/urls&format=xml&sitefilter=enwiki|dewiki";

#if false
            /////// Debugging code: See what the website returns in clear text
            System.Net.WebClient client = new System.Net.WebClient();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            byte[] data = client.DownloadData(request);
            string html = System.Text.Encoding.UTF8.GetString(data);
#endif

            // Added Jan 10, 2020: Set security protocol type in order to avoid message from Wikipedia website
            // that the browser is too old and insecure
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            using (XmlReader xmlReader = XmlReader.Create(request, setgs))
            {
                try
                {
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType == XmlNodeType.Element &&
                            xmlReader.Name == "sitelink" &&
                            xmlReader.GetAttribute("site") == "dewiki")
                        {
                            if (!foundUrl)
                            {
                                deHL.uri = xmlReader.GetAttribute("url");
                                deHL.uri = Uri.UnescapeDataString(deHL.uri);
                                deHL.transMethodUrl = "wikidata";
                            }
                            if (!foundText)
                            {
                                deHL.text = xmlReader.GetAttribute("title");
                                deHL.transMethodText = "wikidata";
                            }
                            // if we added the text translation from the wikidata we regard the translation as partial
                            deHL.transStatus = foundText ? Hyperlink.TranslationStatus.full :
                                Hyperlink.TranslationStatus.partial;
                            return true;
                        }
                    }
                }
                catch (XmlException excp)
                {
                    string msg = excp.Message;
                    MessageBox.Show($"Exception message:\n{msg}", "Exception while reading WikiData", MessageBoxButtons.OK);
                }
            }
            return false;
        }

        private bool FindGermanWikiTopicByPageScan (Hyperlink hl, Hyperlink deHL, bool foundUrl = false, bool foundText = false)
        {
            // download the entire HTML page to search in it for a link to the corresponding German page
            WebClient client = new WebClient();
            string html = client.DownloadString(hl.uri);

            // use HtmlParser to find a href tag that follows a h3 tag with id "p-lng-label"
            HtmlTag tag;
            HtmlParser parser = new HtmlParser(html);
            while (parser.ParseNext("h3", out tag))
            {
                if (!(tag.Attributes.ContainsKey("id") && tag.Attributes["id"] == "p-lang-label"))
                    continue;
                while (parser.ParseNext("a", out tag))
                {
                    if (!(tag.Attributes.ContainsKey("lang") && tag.Attributes["lang"] == "de"))
                        continue;
                    if (!foundUrl && tag.Attributes.ContainsKey("href"))
                    {
                        deHL.uri = Uri.UnescapeDataString(tag.Attributes["href"]);
                        deHL.transMethodUrl = "wikipage";
                    }
                    if (!foundText && tag.Attributes.ContainsKey("title"))
                    {
                        string titleText = tag.Attributes["title"];
                        char dash = titleText[titleText.Length - 8];
                        int idx = titleText.LastIndexOf(" \u2013 German");
                        if (idx > 0)
                            titleText = titleText.Substring(0, idx);
                        deHL.text = titleText;
                        deHL.transMethodText = "wikipage";
                        deHL.transStatus = Hyperlink.TranslationStatus.partial;
                    }
                    return true;
                }
            }
            return false;
        }

#if false
        private void FindRedundantUrlEntries()
        {
            foreach (KeyValuePair<string, string> pair in _urlDict)
            {
                string enUrl = InflateUrl (pair.Key);
                string deUrl = InflateUrl (pair.Value);

                if (deUrl == enUrl)
                {
                    System.Console.WriteLine("untranslated: {0}", enUrl);
                    Hyperlink enLink2 = new Hyperlink();
                    enLink2.uri = enUrl;
                    Hyperlink deLink2 = new Hyperlink();
                    if (FindGermanWikiTopicByWikidata(enLink2, deLink2) ||
                        FindGermanWikiTopicByPageScan(enLink2, deLink2))
                    {
                        string wikiDeUrl = Uri.UnescapeDataString(deLink2.uri);
                        System.Console.WriteLine("   but now has DE: {0}", deLink2.uri);
                    }
                    continue;
                }

                Hyperlink enLink = new Hyperlink();
                enLink.uri = enUrl;
                Hyperlink deLink = new Hyperlink();
                if (FindGermanWikiTopicByWikidata(enLink, deLink) ||
                    FindGermanWikiTopicByPageScan(enLink, deLink))
                {
                    string wikiDeUrl = Uri.UnescapeDataString(deLink.uri);
                    if (wikiDeUrl == deUrl)
                    {
                        System.Console.WriteLine("   -> {0}", enUrl);
                    }
                    else
                    {
                        System.Console.WriteLine("keep: {0} -> {1}", enUrl, deUrl);
                    }
                }
            }
        }

        private void FindCasesForTranslateByPage()
        {
            foreach (KeyValuePair<string, string> pair in _urlDict)
            {
                string enUrl = InflateUrl(pair.Key);
                string deUrl = InflateUrl(pair.Value);

                Hyperlink enLink = new Hyperlink();
                enLink.uri = enUrl;
                Hyperlink deLink = new Hyperlink();
                if (!FindGermanWikiTopicByWikidata(enLink, deLink) &&
                    FindGermanWikiTopicByPageScan(enLink, deLink))
                {
                    System.Console.WriteLine("   -> {0}->{1}", enUrl, deLink.uri);
                    FindGermanWikiTopicByWikidata(enLink, deLink);
                }
            }
        }
#endif

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
            s = s.Replace (",", "");
            s = s.Replace (".", "");
            s = s.Replace (":", "");
            s = s.Replace (";", "");
            s = s.ToLower ();
            return s;
        }

        private string InflateUrl(string url)
        {
            url = url.Replace("$(enWiki)", "https://en.wikipedia.org/wiki/");
            url = url.Replace("$(deWiki)", "https://de.wikipedia.org/wiki/");
            return url;
        }

        private string DeflateUrl(string url)
        {
            url = url.Replace("https://en.wikipedia.org/wiki/", "$(enWiki)");
            url = url.Replace("https://wikipedia.org/wiki/", "$(enWiki)");
            url = url.Replace("http://en.wikipedia.org/wiki/", "$(enWiki)");
            url = url.Replace("http://wikipedia.org/wiki/", "$(enWiki)");

            url = url.Replace("https://de.wikipedia.org/wiki/", "$(deWiki)");
            url = url.Replace("http://de.wikipedia.org/wiki/", "$(deWiki)");
            return url;
        }

        private string EscapeSemicolons (string s)
        {
            s = s.Replace("%", "%25");
            return s.Replace(";", "%3B");
        }

        private string UnescapeSemicolons(string s)
        {
            s = s.Replace("%3B", ";"); 
            return s.Replace("%25", "%");
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
