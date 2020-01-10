using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkTranslator.Properties;
using System.Windows.Forms;

namespace LinkTranslator
{
    static class Helpers
    {
        static public string AppDataFolder
        {
            get {return Settings.Default.appDataFolderPath;}
        }

        static public bool CheckDirectoryWriteAccess (string dirPath)
        {
            try
            {
                string testFilePath = Path.Combine(dirPath, Path.GetRandomFileName());
                using (FileStream fs = File.Create (testFilePath, 1, FileOptions.DeleteOnClose))
                    { } // no-op
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static void CheckSettingsUpgrade()
        {
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }
        }
    }
}
