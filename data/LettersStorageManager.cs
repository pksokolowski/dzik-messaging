using Dzik.crypto.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.data
{
    internal static class LettersStorageManager
    {
        private static string LettersRootDir = StorageManager.DataFolderPath + "letters";
        private static string LettersOutputDir = LettersRootDir + "/outbound";

        private static string InboundPath = LettersRootDir + "/inbound";
        private static string DraftPath = LettersRootDir + "/draft";
        private static string OutputLetterPath = LettersOutputDir + "/milkBottle";


        private static void EnsureLettersDataFolderExists()
        {
            if (!Directory.Exists(LettersRootDir)) Directory.CreateDirectory(LettersRootDir);
        }

        private static void EnsureLettersOutpoutFolderExists()
        {
            EnsureLettersDataFolderExists();
            if (!Directory.Exists(LettersOutputDir)) Directory.CreateDirectory(LettersOutputDir);
        }

        // writes

        internal static void StoreInboundMessage(byte[] inboundMsg)
        {
            EnsureLettersDataFolderExists();
            File.WriteAllBytes(InboundPath, inboundMsg);
        }

        internal static void StoreDraftMessage(byte[] draftMsg)
        {
            EnsureLettersDataFolderExists();
            File.WriteAllBytes(DraftPath, draftMsg);
        }

        internal static void SaveOutboundMessage(byte[] outboundMsg)
        {
            EnsureLettersOutpoutFolderExists();
            File.WriteAllBytes(OutputLetterPath, outboundMsg);
        }

        // reads

        internal static byte[] ReadInboundMessage()
        {
            EnsureLettersDataFolderExists();
            return File.ReadAllBytes(InboundPath);
        }

        internal static byte[] ReadDraftMessage()
        {
            EnsureLettersDataFolderExists();
            return File.ReadAllBytes(DraftPath);
        }

        internal static void OpenOutputLetterFolder()
        {
            WindowsExplorerOpener.ShowFileInExplorer(OutputLetterPath);
        }

        // deletes

        internal static void DeleteDraft()
        {
            EnsureLettersDataFolderExists();
            File.Delete(DraftPath);
        }

        internal static void DeleteInbound()
        {
            EnsureLettersDataFolderExists();
            File.Delete(InboundPath);
        }

        // checks 

        internal static bool IsDraftStored()
        {
            EnsureLettersDataFolderExists();
            return File.Exists(DraftPath);
        }

        internal static bool IsInboundStored()
        {
            EnsureLettersDataFolderExists();
            return File.Exists(InboundPath);
        }
    }
}
