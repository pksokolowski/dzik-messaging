using Dzik.crypto.algorithms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;

namespace Dzik.crypto.protocols
{
    internal static class KeyStorage
    {
        internal static bool IsKeyStored(string KeyFilePath)
        {
            return File.Exists(KeyFilePath);
        }

        internal static bool StoreKey(string KeyFilePath, byte[] key, string password)
        {
            try
            {
                var encryptedKey = PasswordBasedEncryptor.Encrypt(key, password);
                File.WriteAllBytes(KeyFilePath, encryptedKey);
            }
            catch
            {
                return false;
            }
            return true;
        }

        internal static bool StoreKey(string KeyFilePath, byte[] key, byte[] KEK)
        {
            try
            {
                var encryptedKey = AesTool.Encrypt(KEK, key);
                File.WriteAllBytes(KeyFilePath, encryptedKey);
            }
            catch
            {
                return false;
            }
            return true;
        }

        internal static byte[] ReadKeyBytes(string KeyFilePath, string password)
        {
            var keyFileContents = File.ReadAllBytes(KeyFilePath);
            return PasswordBasedEncryptor.Decrypt(keyFileContents, password);
        }

        internal static PinnedBytes ReadPinnedKeyBytes(string KeyFilePath, SecureString password)
        {
            var keyFileContents = File.ReadAllBytes(KeyFilePath);
            return PasswordBasedEncryptor.DecryptPinned(keyFileContents, password);
        }

        internal static byte[] ReadKeyBytes(string KeyFilePath, byte[] KEK)
        {
            var keyFileContents = File.ReadAllBytes(KeyFilePath);
            return AesTool.Decrypt(KEK, keyFileContents);
        }

        internal static PinnedBytes ReadPinnedKeyBytes(string KeyFilePath, byte[] KEK)
        {
            if (!IsKeyStored(KeyFilePath)) return null;

            var keyFileContents = File.ReadAllBytes(KeyFilePath);
            return AesTool.DecryptPinned(KEK, keyFileContents);
        }

        internal static bool RemoveKey(string KeyFilePath)
        {
            try
            {
                File.Delete(KeyFilePath);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        /// <returns>key used for encrypting the data</returns>
        internal static byte[] BackupTheKey(string KeyFilePath, string backupFilePath)
        {
            try
            {
                if (!IsKeyStored(KeyFilePath)) return null;

                var fileBytes = File.ReadAllBytes(KeyFilePath);

                var (encryptedBytes, key) = RandomKeyBasedEncryptor.Encrypt(fileBytes);
                if (encryptedBytes == null) return null;

                File.WriteAllBytes(backupFilePath, encryptedBytes);

                return key;
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static bool RestoreBackup(string KeyFilePath, string path, byte[] backupKey)
        {
            if (IsKeyStored(KeyFilePath)) return false;

            var fileBytes = File.ReadAllBytes(path);

            var decryptedFileBytes = RandomKeyBasedEncryptor.Decrypt(backupKey, fileBytes);
            if (decryptedFileBytes == null) return false;

            File.WriteAllBytes(KeyFilePath, decryptedFileBytes);

            return true;
        }
    }
}
