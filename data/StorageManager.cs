﻿using Dzik.crypto.protocols;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.data
{
    internal static class StorageManager
    {
        private static string DataFolderPath = "dzik-data";
        private static string DraftFilePath = DataFolderPath + "/draft.txt";
        private static string MasterKeysPath = DataFolderPath + "/keys";

        private static byte[] MinimalDefenceKey = new byte[32] { 200, 3, 8, 3, 144, 3, 3, 77, 3, 144, 3, 3, 23, 3, 3, 12, 120, 3, 2, 3, 144, 3, 3, 77, 5, 144, 33, 163, 23, 111, 3, 121 };

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

        internal static PinnedBytes ReadMasterKeys()
        {
            EnsureDataFolderExists();
            return KeyStorage.ReadPinnedKeyBytes(MasterKeysPath, MinimalDefenceKey);
        }

        internal static bool WriteMasterKeys(byte[] masterKeys)
        {
            EnsureDataFolderExists();
            return KeyStorage.StoreKey(MasterKeysPath, masterKeys, MinimalDefenceKey);
        }
    }
}
