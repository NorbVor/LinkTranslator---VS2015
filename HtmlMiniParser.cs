using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LinkTranslator
{
    class HtmlMiniParser
    {
        // our input string and the current reading position
        string _input = null;
        int _pos = -1;

        // temporary storage for the non-digested contents of the current tag;
        // we used it to extract the attributes replace its contents with the
        // remaining part that has not been scanned yet.
        string _tag;

        // the most recent object that we have seen; used to feed the properties
        // of this class.
        string _tagName;
        string _attrName;
        string _attrValue;
        string _intraTag;

        /**********************************************************************
        *** public interface
        **********************************************************************/

        public HtmlMiniParser (string s)
        {
            _input = s;
            _pos = 0;
        }

        /// <summary>
        /// Read the next <tagname ...> tag from the input stream. The entire tag
        /// including the included attributes and the following intra-tag contents
        /// are read and stored in private members.
        /// </summary>
        /// <returns>The tag name</returns>
        public bool ReadTag ()
        {
            _tagName = string.Empty;
            _tag = string.Empty;
            _intraTag = string.Empty;
            StringBuilder sb = new StringBuilder ();

            if (!SkipIncluding ('<'))
                return false;

            // read tag name
            _tagName = ConsumeIntagWord ();
            if (AtEnd ())
                return false; // premature tag end

            // read attributes
            while (!AtEnd ())
            {
                SkipWhiteSpace ();
                if (Peek () == '>')
                {
                    GetChar (); // tag end
                    break;
                }
                else if (Peek () == '/')
                {
                    GetChar (); // auto closing tag
                    if (GetChar () != '>')
                    {
                        // mal-formed tag
                        return false;
                    }
                    break;
                }
                else
                {
                    // tag contains one or more attributes; read the entire
                    // string up the '>' into _tag
                    sb.Length = 0;
                    while (!AtEnd ())
                    {
                        if (Peek () == '>')
                            break;
                        sb.Append (GetChar ());
                    }
                    _tag = sb.ToString ().Trim ();
                }
            }

            // read tha intra tag text up to the next tag
            sb.Length = 0;
            while (!AtEnd () && Peek () != '<')
                sb.Append (GetChar ());
            _intraTag = sb.ToString ();
            ReduceWhitespace (ref _intraTag);

            // success
            return true;
        }

        /// <summary>
        /// After we have read a tag we can successively read its attributes.
        /// Our reading position in the input string is already on the start of the
        /// next tag. But we have temporarily stored the contents after the tag name
        /// in our member variable _tag. For the purpose of scanning we can't use _pos
        /// but use a local variable idx. Before we return from the function we cut off
        /// the read attribute string from the start of _tag to prepare if for the next
        /// attribute retrieval. If all attributes have been consumed we return false.
        /// </summary>
        /// <returns>True if an attribute could be consumed. The attribute's name
        /// name and value can be accessed via the corresponding properties.</returns>
        public bool ReadAttribute ()
        {
            _attrName = string.Empty;
            _attrValue = string.Empty;

            if (_tag.Length == 0)
                return false;

            // read attribute name
            int idx = 0;
            StringBuilder sb = new StringBuilder ();
            while (idx < _tag.Length && !char.IsWhiteSpace (_tag[idx]) && _tag[idx] != '=')
            {
                sb.Append (_tag[idx++]);
            }
            _attrName = sb.ToString ();
            if (string.IsNullOrEmpty (_attrName) || _tag[idx] != '=')
            {
                _attrName = "";
                _tag = string.Empty;
                return false;
            }

            // read attribute value
            ++idx; // skip over the '='
            sb.Length = 0; //start collecting the attribute value
            if (_tag[idx] == '"')
                GetStringValueInTag (ref idx, sb);
            _attrValue = sb.ToString ();
            SkipWhiteSpaceInTag (ref idx);
            _tag = _tag.Substring (idx);

            // success
            return true;
        }

        public string TagName
        { get { return _tagName; } }

        public string AttributeName
        { get { return _attrName; } }

        public string AttributeValue
        { get { return _attrValue; } }

        public string IntraTagText
        { get { return _intraTag; } }

        /**********************************************************************
        *** Scanner functions
        **********************************************************************/
        private bool AtEnd ()
        {
            return _pos >= _input.Length;
        }

        private char GetChar ()
        {
            return _input[_pos++];
        }

        private char Peek ()
        {
            return _input[_pos];
        }

        private void SkipUpTo (char stopper)
        {
            while (!AtEnd() && _input[_pos] != stopper)
                ++_pos;
        }

        private bool SkipIncluding (char stopper)
        {
            while (!AtEnd () && _input[_pos++] != stopper)
                ;
            return !AtEnd ();
        }

        private void SkipWhiteSpace ()
        {
            while (!AtEnd () && char.IsWhiteSpace (_input[_pos]))
                _pos++;
        }

        /// <summary>
        /// Assumes that we are inside a tag and consumes all characters up to the
        /// next whitespace or end of tag and returns them as a string value.
        /// Is typically used to read the tag name.
        /// </summary>
        /// <returns>The consumed word</returns>
        private string ConsumeIntagWord ()
        {
            StringBuilder sb = new StringBuilder ();
            while (!AtEnd () && !char.IsWhiteSpace (_input[_pos]) && _input[_pos] != '>')
                sb.Append (_input[_pos++]);
            return sb.ToString ();
        }

        ////////////////////
        // functions that operate on the _tag string and use a separate idx for scanning
        ////////////////////

        /// <summary>
        /// Skips all whitespace in _tag starting from the current position in idx. Upon
        /// return idx points to the next non-whitespace character or just beyond the string
        /// end.
        /// </summary>
        /// <param name="idx"></param>
        private void SkipWhiteSpaceInTag (ref int idx)
        {
            while (idx < _tag.Length && char.IsWhiteSpace (_tag[idx]))
                ++idx;
        }

        /// <summary>
        /// Assumes that idx points to the opening quote of a string inside _tag. In copies
        /// the entire string value (without the closing quote) to the StringBuilder sb,
        /// thereby performing the necessare escapes.
        /// Note: In this simplified implementation we only do backslash escapes. The HTML
        /// escapes &nnn &amp etc. are not implemented yet.
        /// </summary>
        /// <param name="idx">The index of the opening quite inside _tag</param>
        /// <param name="sb">The StringBuilder in which we accumulate the string value</param>
        private void GetStringValueInTag (ref int idx, StringBuilder sb)
        {
            ++idx; // skip the leading quote
            while (idx < _tag.Length && _tag[idx] != '"')
            {
                if (_tag[idx] == '\\')
                {
                    ++idx;
                    if (idx >= _tag.Length)
                    {
                        sb.Append ('"'); // forced closing of string
                        return;
                    }
                }
                sb.Append (_tag[idx++]);
            }
            ++idx;
        }

        /// <summary>
        /// Reduces in-place any sequence of one or more whitespace characters
        /// to a singl space.
        /// </summary>
        /// <param name="s"></param>
        private static void ReduceWhitespace (ref string s)
        {
            bool inWhite = false;
            StringBuilder sb = new StringBuilder ();
            foreach (char c in s)
            {
                if (char.IsWhiteSpace (c))
                {
                    if (!inWhite)
                    {
                        sb.Append (' ');
                        inWhite = true;
                    }
                }
                else
                {
                    sb.Append (c);
                    inWhite = false;
                }
            }
            s = sb.ToString ();
        }
    }
}
