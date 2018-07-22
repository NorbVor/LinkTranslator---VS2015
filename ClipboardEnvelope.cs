using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LinkTranslator
{
    /*
     * When an HTML link is placed on the clipboard or transferred via drag-and-drop it
     * is preceded by a few lines that help find the HTML part and the fragment with the
     * <A href=...> element. I call these an evelope. This class is a helper that 
     * can parse these starter lines and then give access to the contained full HTML and
     * the link fragment. It also contains a static function that can construct such an
     * envelope and HTML for a given hyperlink.
     * */

    class ClipboardEnvelope
    {
        private int _startHtml;
        private int _endHtml;
        private int _startFragment;
        private int _endFragment;

        private string _envString;
        static private string[] _labels = { "starthtml", "endhtml", "startfragment", "endfragment" };

        public bool Read (string envString)
        {
            // initialize all members
            _startHtml = _endHtml = _startFragment = _endFragment = -1;
            _envString = envString;

            // often the clipboard data contains UTF8 encoded strings without specifying so
            if (Environment.Version.Major < 4)
                FixMisencodedUTF8 (envString, out _envString);

            StringReader textReader = new StringReader (envString);
            string line;
            while ((line = textReader.ReadLine ()) != null)
            {
                // if line starts with '<' as first non-blank charcter, we have
                // consumend the entire header
                if (line.Trim ().StartsWith ("<"))
                    break;

                // parse the line according to the scheme: label:value
                string[] comps = line.Split (':');
                string label = comps[0].ToLower ();
                int idx;
                int value = -1;
                if ((idx = Array.IndexOf (_labels, label)) >= 0)
                    value = int.Parse (comps[1]);

                // interpret the label and put the value into the according member
                switch (idx)
                {
                    case 0: _startHtml = value; break;
                    case 1: _endHtml = value; break;
                    case 2: _startFragment = value; break;
                    case 3: _endFragment = value; break;
                }
            }
            if (_startHtml < 0 || _endHtml < 0 || _startFragment < 0 || _endFragment < 0)
                return false;

            // fix _endHtml, because it OpenOffice sometimes puts it too hight by 1
            if (_endHtml > envString.Length)
                _endHtml = envString.Length;

            // fix the offsets, because they referred to the UTF-8 encoding
            if (Environment.Version.Major < 4 && envString.Length != _envString.Length)
            {
                byte[] data = Encoding.Default.GetBytes (envString);
                _startHtml      = FixUtf8Offset (data, _startHtml);
                _endHtml        = FixUtf8Offset (data, _endHtml);
                _startFragment  = FixUtf8Offset (data, _startFragment);
                _endFragment    = FixUtf8Offset (data, _endFragment);
            }

            return true;
        }

        public string GetHtmlString ()
        {
            if (_startHtml < 0 || _endHtml < 0)
                return null;
            return _envString.Substring (_startHtml, _endHtml - _startHtml);
        }

        public string GetFragmentString ()
        {
            if (_startFragment < 0 || _endFragment < 0)
                return null;
            return _envString.Substring (_startFragment, _endFragment - _startFragment);
        }

        public static string BuildHtmlClipboardEnvelope (string htmlFragment)
        {
            StringBuilder sb = new StringBuilder (1000);
            sb.Append ("Version:0.9\r\n");
            sb.Append ("StartHTML:<ssssssss>\r\n");
            sb.Append ("EndHTML:<eeeeeeee>\r\n");
            sb.Append ("StartFragment:<ffffffff>\r\n");
            sb.Append ("EndFragment:<gggggggg>\r\n");

            int startHtml = sb.Length;
            sb.Append ("<HTML><BODY DIR=\"LTR\">\r\n");
            sb.Append ("<!--StartFragment -->");
            int startFrag = GetByteCount (sb);
            sb.Append (htmlFragment);
            int endFrag = GetByteCount (sb);
            sb.Append ("<!--EndFragment -->");
            sb.Append ("\r\n</BODY></HTML>");
            int endHtml = GetByteCount (sb);

            sb.Replace ("<ssssssss>", startHtml.ToString ("D10"));
            sb.Replace ("<eeeeeeee>", endHtml.ToString ("D10"));
            sb.Replace ("<ffffffff>", startFrag.ToString ("D10"));
            sb.Replace ("<gggggggg>", endFrag.ToString ("D10"));
            string s = sb.ToString ();

            // re-encode the string so it will work  correctly (fixed in CLR 4.0)      
            if (Environment.Version.Major < 4 && s.Length != Encoding.UTF8.GetByteCount (s))
                s = Encoding.Default.GetString (Encoding.UTF8.GetBytes (s));
            return s;
        }

        public static bool FixMisencodedUTF8 (string inText, out string outText)
        {
            outText = string.Empty;
            if (string.IsNullOrEmpty (inText))
                return false;
            byte[] data = Encoding.Default.GetBytes (inText);
            outText = Encoding.UTF8.GetString (data);
            return true;
        }

        /// <summary>      
        /// Calculates the number of bytes produced by encoding the string in the string builder in UTF-8
        /// and not .NET default string encoding.      
        /// </summary>      
        /// <param name="sb">the  string builder to count its string</param>      
        /// <returns>the number of bytes  required to encode the string in UTF-8</returns>      
        private static int GetByteCount (StringBuilder sb)      
        {      
            int end = sb.Length;
            int count = 0;
            for (int i = 0; i < end; i++)      
            {      
                _byteCount[0] = sb[i];      
                count +=  Encoding.UTF8.GetByteCount (_byteCount);      
            }      
            return count;      
        }      

        /// <summary>      
        /// Used to calculate characters byte count  in UTF-8      
        /// </summary>      
        private static readonly char[] _byteCount =  new char[1];

        private static int FixUtf8Offset (byte[] data, int offset)
        {
            int v = Encoding.UTF8.GetCharCount (data, 0, offset);
#if DEBUG
            if (v != offset)
                System.Console.WriteLine ("Offset {0} fixed to {1}", offset, v);
#endif
            return v;
        }
     }
}
