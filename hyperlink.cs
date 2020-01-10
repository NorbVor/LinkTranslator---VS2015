using System;
using System.Collections.Generic;
using System.Text;

namespace LinkTranslator
{
    /// <summary>
    /// The Hyperlink class is used to represent a hyperlink of OpenOffice or an HTML
    /// document. The two main components are:
    /// 
    /// a) the text representation of the link
    /// b) the URL that is to be displayed when the user clicks the link
    /// 
    /// In addition we store information about the translation process, i.e. which
    /// method has translated the link and was the translation successful, partially
    /// successful or unsuccessful.
    /// </summary>
    class Hyperlink : ICloneable
    {
        public string text = "";
        public string uri = "";

        public string transMethodText = "";
        public string transMethodUrl = "";
        public TranslationStatus transStatus = TranslationStatus.notrans;
        public enum TranslationStatus { notrans, partial, full, eso };
        static readonly string[] msgs = { "(\u2013)", "(P)", "(F)", "(ESO)" };

        public bool uriChanged = false;
        public bool textChanged = false;

        /// <summary>
        /// Returns a string showing the translation status of the hyperlink.
        /// </summary>
        public string StatusMsg
        {
            get {return msgs[(int)transStatus];}
        }

        /// <summary>
        /// Returns a shallow copy of this object.
        /// </summary>
        public object Clone ()
        {
            return this.MemberwiseClone ();
        }
    }
}
