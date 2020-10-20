using System.Resources;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle ("Link Translator")]
[assembly: AssemblyDescription ("This utility translates hyperlinks in OpenOffice " +
"documents from English to German. For all Wikipedia links it " +
"consults the Wikidata service and finds their German equivalent " +
"if available. In addition it applies a manually generated translation " +
"database to translate URLs and link text. The translated links are either appended " +
"at the end of the originating OpenOffice document or can be transferred by drag and drop " +
"to the translation file.")]
[assembly: AssemblyConfiguration ("")]
[assembly: AssemblyCompany ("Norbert_Vorstädt")]
[assembly: AssemblyProduct ("Link Translator")]
[assembly: AssemblyCopyright ("Copyright ©  Norbert Vorstädt, 2018, 2019, 2020")]
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible (false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid ("350f147f-f817-4df3-af76-20f2f11be372")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion ("1.3.*")]
[assembly: AssemblyFileVersion ("1.3.0.0")]
[assembly: NeutralResourcesLanguage("")]

