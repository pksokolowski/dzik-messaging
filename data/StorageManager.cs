using Dzik.crypto.protocols;
using Dzik.crypto.utils;
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
        private static string TempSecretDataFolderPath = "dzik-temp-priv-data";

        private static string DraftFilePath = DataFolderPath + "/draft.txt";
        private static string EncryptedDraftFilePath = DataFolderPath + "/edraft";

        private static string MasterKeysPath = DataFolderPath + "/keys";
        private static string EncryptedMasterKeysPath = DataFolderPath + "/ekeys";

        private static string KeyAgreementChallengePath = DataFolderPath + "/challenge-Share";
        private static string KeyAgreementPrivateKeyPath = TempSecretDataFolderPath + "/PrivateTemp-DoNotShare";
        private static string KeyAgreementResponsePath = DataFolderPath + "/Response-Share.txt";

        private static byte[] MinimalDefenceKey = new byte[32] { 200, 3, 8, 3, 144, 3, 3, 77, 3, 144, 3, 3, 23, 3, 3, 12, 120, 3, 2, 3, 144, 3, 3, 77, 5, 144, 33, 163, 23, 111, 3, 121 };

        private static void EnsureDataFolderExists()
        {
            if (!Directory.Exists(DataFolderPath)) Directory.CreateDirectory(DataFolderPath);
        }

        private static void EnsureTempPrivDataFolderExists()
        {
            if (!Directory.Exists(TempSecretDataFolderPath)) Directory.CreateDirectory(TempSecretDataFolderPath);
        }


        // Drafts:

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

        internal static void SaveEncryptedDraft(byte[] content)
        {
            EnsureDataFolderExists();
            if (content.Length == 0)
            {
                File.Delete(EncryptedDraftFilePath);
                return;
            }
            File.WriteAllBytes(EncryptedDraftFilePath, content);
        }

        internal static byte[] ReadEncryptedDraft()
        {
            EnsureDataFolderExists();
            return File.ReadAllBytes(EncryptedDraftFilePath);
        }


        // Master keys:

        internal enum MasterKeysState { UNPROTECTED, PASSWD_PROTECTED, NOT_PRESENT }
        internal static MasterKeysState GetMasterKeysState()
        {
            if (File.Exists(MasterKeysPath))
            {
                return MasterKeysState.UNPROTECTED;
            }

            if (File.Exists(EncryptedMasterKeysPath))
            {
                return MasterKeysState.PASSWD_PROTECTED;
            }

            return MasterKeysState.NOT_PRESENT;
        }

        internal static PinnedBytes ReadPasswordProtectedMasterKeys(SecureString passwd)
        {
            EnsureDataFolderExists();
            return KeyStorage.ReadPinnedKeyBytes(EncryptedMasterKeysPath, passwd);
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

        internal static bool WritePasswordProtectedMasterKeys(byte[] masterKeys, string password)
        {
            EnsureDataFolderExists();
            return KeyStorage.StoreKey(EncryptedMasterKeysPath, masterKeys, password);
        }


        // Key Agreement Challenge:

        internal static byte[] ReadKeyAgreementChallengeOrNull()
        {
            EnsureDataFolderExists();
            if (!File.Exists(KeyAgreementChallengePath)) return null;
            return KeyStorage.ReadKeyBytes(KeyAgreementChallengePath, MinimalDefenceKey);
        }

        internal static void WriteKeyAgreementChallenge(byte[] challenge, bool openContainingFolder)
        {
            EnsureDataFolderExists();
            KeyStorage.StoreKey(KeyAgreementChallengePath, challenge, MinimalDefenceKey);
        }

        internal static void OpenChallengeLocationInExplorer()
        {
            WindowsExplorerOpener.ShowFileInExplorer(DataFolderPath);
        }


        // Key Agreement Private key

        internal static byte[] ReadKeyAgreementPrivateKeyOrNull()
        {
            if (!File.Exists(KeyAgreementPrivateKeyPath)) return null;
            return KeyStorage.ReadKeyBytes(KeyAgreementPrivateKeyPath, MinimalDefenceKey);
        }

        internal static void WriteKeyAgreementPrivateKey(byte[] privateKey)
        {
            EnsureTempPrivDataFolderExists();
            KeyStorage.StoreKey(KeyAgreementPrivateKeyPath, privateKey, MinimalDefenceKey);
        }


        // Key Agreement Response:

        internal static void WriteKeyAgreementResponse(string response)
        {
            EnsureDataFolderExists();
            File.WriteAllText(KeyAgreementResponsePath, response);
        }

        internal static string ReadKeyAgreementResponse()
        {
            EnsureDataFolderExists();
            if (!File.Exists(KeyAgreementResponsePath)) return null;
            return File.ReadAllText(KeyAgreementResponsePath);
        }
    }
}
