using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LinkTranslator
{
    /*
     * This class contains an auxiliary functions to collect valid 
     * hyperlink translations from an existing document in order to help
     * build a link database.
     * */
    class LinkCollector
    {
        private StreamWriter _outStream;
        private StreamWriter _msgStream;

        /// <summary>
        /// Loop over all OpenOffice Writer files in a given directory (at 
        /// the moment "D:\ESO Übersetzungen") and call CollectLinksFromDocument
        /// on them. 
        /// 
        /// All the contained hyperlinks and their translations are recorded in
        /// a CSV files "CollectedLinks.txt".
        /// </summary>
        public void Execute ()
        {
            _outStream = File.CreateText ("CollectedLinks.txt");
            _msgStream = File.CreateText ("MessageLog.txt");

            string[] files = Directory.GetFiles ("D:\\ESO Übersetzungen", "*.odt");
            foreach (string filePath in files)
                CollectLinksFromDocument (filePath);

            _outStream.Close ();
            _msgStream.Close ();
        }

        /// <summary>
        /// Collect links and their translations from an OpenOffice Writer file.
        /// We assume that the files contains first the English text, then the translated
        /// German text. We build on the fact that the sequence of hypeslinks is unchanged
        /// in the translation so that we can associate each link with its translated version.
        /// </summary>
        /// <param name="filePath">File path to the input document.</param>
        private void CollectLinksFromDocument (string filePath)
        {
            OpenOfficeDoc doc = new OpenOfficeDoc ();
            if (!doc.Read (filePath))
                return;

            List<Hyperlink> links = new List<Hyperlink> ();
            if (!doc.ExtractLinks (links))
                return;

            if (!doc.ContainsLangSeparator ())
                return;

            // fold link list in half and treat the second half as translations
            // of the first half
            if (links.Count % 2 != 0)
            {
                _msgStream.WriteLine ("{0}: Number of links in document is not even! Skipping this document.",
                    filePath);
                return;
            }
            int splitIndex = links.Count / 2;
            int remaining = links.Count - splitIndex;
            List<Hyperlink> deLinks = links.GetRange (splitIndex, remaining);
            links.RemoveRange (splitIndex, remaining);

            // loop over both lists in sync and record potential translation candidates in
            // text lines of the format
            //
            //   <english-text>; <english-link>; <german-text>; <german-link>
            //
            // We only collect links to Wikipedia.org!
            //
            for (int i = 0; i < splitIndex; ++i)
            {
                Hyperlink enLink = links[i];
                Hyperlink deLink = deLinks[i];

                Uri uri = new Uri (enLink.uri);
                if (uri.Authority == "en.wikipedia.org")
                {
                    _outStream.WriteLine ("{0};{1};{2};{3}",
                        enLink.text, enLink.uri, deLink.text, deLink.uri);
                }
            }
        }
    }
}
