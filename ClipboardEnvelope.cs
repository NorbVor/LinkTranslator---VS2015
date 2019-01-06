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
                FixMisencodedUTF8(envString, out _envString);

#if false
            if (!DecodeSectionHeader ())
                return false;

            // save the offsets found by DecodeSectionHeader
            int saveStartHtml = _startHtml;
            int saveEndHtlm = _endHtml;
            int saveStartFragment = _startFragment;
            int saveEndFragment = _endFragment;
#endif
            // we don't rely on the offset fields of the header, but determine our own offsets by
            // search for the HTML-tags
            if (!FindHtmlAndFragmentOffsets())
                return false;

#if false
            DebugPrintOffsetDiscrepancies(saveStartHtml, saveEndHtlm, saveStartFragment, saveEndFragment);
#endif
            return true;
        }

        // find the HTML and fragment start/end by searching for the HTML tags
        private bool FindHtmlAndFragmentOffsets()
        {
            _startHtml = GetStartOfTagText(_envString, "html");
            _startFragment = GetStartOfTagText(_envString, "body");
            _endFragment = GetEndOfTagText(_envString, "body");
            _endHtml = GetEndOfTagText(_envString, "html");
            if (!(_startHtml >= 0 &&_startHtml < _startFragment && _startFragment <= _endFragment && _endFragment < _endHtml))
                return false;
            return true;
        }

        // decode the header section
        private bool DecodeSectionHeader ()
        {
            StringReader textReader = new StringReader(_envString);
            string line;
            while ((line = textReader.ReadLine()) != null)
            {
                // if line starts with '<' as first non-blank charcter, we have
                // consumend the entire header
                if (line.Trim().StartsWith("<"))
                    break;

                // parse the line according to the scheme: label:value
                string[] comps = line.Split(':');
                string label = comps[0].ToLower();
                int idx;
                int value = -1;
                if ((idx = Array.IndexOf(_labels, label)) >= 0)
                    value = int.Parse(comps[1]);

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
            if (!(_startHtml < _startFragment && _startFragment <= _endFragment && _endFragment < _endHtml))
                return false;

            // fix the offsets, because they referred to the UTF-8 encoding
            return FixEnvelopeOffsets();
        }

        private int GetStartOfTagText (string html, string tag)
        {
            int startOfTag = html.IndexOf("<" + tag, 0, StringComparison.CurrentCultureIgnoreCase);
            if (startOfTag < 0)
                return startOfTag;
            int endOfTag = html.IndexOf(">", startOfTag);
            if (endOfTag < 0)
                return endOfTag;
            return endOfTag + 1;
        }

        private int GetEndOfTagText (string html, string tag)
        {
            return html.IndexOf("</" + tag, 0, StringComparison.CurrentCultureIgnoreCase);
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

        /// <summary>
        /// The envelope header fields (_startHtml, ...) contain offsets into the original UTF8 string
        /// which the framework has already converted for us into a UTF16 string. But the framework didn't fix
        /// the header fields accordingly. So we must correct them as all 2- and 3-bytes sequences in UTF8 have
        /// become a single UTF16 character in our new string.
        /// </summary>
        private bool FixEnvelopeOffsets ()
        {
            // as a precaution we check that all entries are within the UTF8 string boundaries
            int utf8Length = Encoding.UTF8.GetByteCount(_envString);
            if (_startHtml >= utf8Length || _endHtml > utf8Length ||
                _startFragment >= utf8Length || _endFragment >= utf8Length)
                    return false;

            // if utf8Length is the same as our UTF16 string length the string did not contain 
            // any 2- or 3-byte sequences, so we are done
            if (utf8Length == _envString.Length)
                return true; 

            // convert the UTF8 index values segment by segment
            int i1 = ConvUtf8IdxToUtf16Idx(_envString, 0, 0, _startHtml);
            int i2 = ConvUtf8IdxToUtf16Idx(_envString, _startHtml, i1, _startFragment);
            int i3 = ConvUtf8IdxToUtf16Idx(_envString, _startFragment, i2, _endFragment);
            int i4 = ConvUtf8IdxToUtf16Idx(_envString, _endFragment, i3, _endHtml);

            _startHtml = i1;
            _startFragment = i2;
            _endFragment = i3;
            _endHtml = i4;

            return true;
        }

        int ConvUtf8IdxToUtf16Idx (string s, int fromUtf8Idx, int fromUtf16Idx, int toUtf8Idx)
        {
            int utf16Idx = fromUtf16Idx;
            for (int utf8idx = fromUtf8Idx; utf8idx < toUtf8Idx;)
            {
                char c = s[utf16Idx++];
                int utf8Length = (c & ~0x7f) == 0 ? 1 : (c & ~0x7ff) == 0 ? 2 : 3;
                utf8idx += utf8Length;
            }
            return utf16Idx;
        }

        /*
        ******************************** Debugging Helpers ************************
        */
#if DEBUG
        private void DebugPrintOffsetDiscrepancies (int saveStartHtml, int saveEndHtml, int saveStartFragment, int saveEndFragment)
        {
            Console.WriteLine("Discrepancies between offsets from header fields and text search:");
            Console.WriteLine("StartFragment discrepancy {0}", _startFragment - saveStartFragment);
            Console.WriteLine("EndFragment discrepancy   {0}", _endFragment - saveEndFragment);

            DebugPrintOffset("startHtlm", _startHtml, saveStartHtml);
            DebugPrintOffset("startFragment", _startFragment, saveStartFragment);
            DebugPrintOffset("endFragment", _endFragment, saveEndFragment);
            DebugPrintOffset("endHtlm", _endHtml, saveEndHtml);
        }

        private void DebugPrintOffset (string name, int offsetA, int offsetB)
        {
            System.Console.WriteLine(name + ":");
            int startIdx = Math.Min(offsetA, offsetB) - 5;
            startIdx = Math.Max(startIdx, 0);
            int limIdx = Math.Max(offsetA, offsetB) + 5;
            limIdx = Math.Min(limIdx, _envString.Length);

            DumpString(_envString, startIdx, limIdx);

            System.Console.Write("    ");
            int marker1 = offsetA;
            int marker2 = offsetB;
            char c1 = 'A';
            char c2 = 'B';
            if (offsetB < offsetA)
            {
                marker1 = offsetB;
                marker2 = offsetA;
                c1 = 'B';
                c2 = 'A';
            } else if (offsetA == offsetB)
            {
                c1 = '|';
            }

            int idx = startIdx;
            for (; idx < marker1; ++idx)
                System.Console.Write("   ");
            System.Console.Write(" {0}  ", c1);
            ++idx;
            if (offsetA != offsetB)
            {
                for (; idx < marker2; ++idx)
                    System.Console.Write("   ");
                System.Console.Write(" {0}  ", c2);
            }
            System.Console.WriteLine("");
            System.Console.WriteLine("");
        }

        private void DumpString(string s, int startIdx, int endIdx)
        {
            System.Console.Write("    ");
            for (int idx = startIdx; idx < endIdx; ++idx)
                System.Console.Write(Convert.ToInt16(s[idx]).ToString("X2") + " ");
            System.Console.WriteLine("");

            System.Console.Write("    ");
            for (int idx = startIdx; idx < endIdx; ++idx)
            {
                if (char.IsControl(s[idx]))
                    System.Console.Write(" . ");
                else
                    System.Console.Write(" " + s[idx] + " ");
            }
            System.Console.WriteLine("");
        }
#endif
    }
}
