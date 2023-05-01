using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.data
{
    internal static class StorageManager
    {
        private static string DataFolderPath = "dzik-data";
        private static string DraftFilePath = DataFolderPath+"/draft.txt";

        private static void EnsureDataFolderExists()
        {
            if (!Directory.Exists(DataFolderPath)) Directory.CreateDirectory(DataFolderPath);
        }

        internal static void SaveDraft(String content)
        {
            EnsureDataFolderExists();
            File.WriteAllText(DraftFilePath, content);
        }

        internal static string ReadDraft()
        {
            EnsureDataFolderExists();
            return File.ReadAllText(DraftFilePath);
        }
    }
}
