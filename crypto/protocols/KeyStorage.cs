using Dzik.crypto.algorithms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.protocols
{
    internal class KeyStorage
    {
        private string KeyFilePath;
        public KeyStorage(string keyFilePath)
        {
            KeyFilePath = keyFilePath;
        }

        internal bool IsKeyStored()
        {
            return File.Exists(KeyFilePath);
        }

        internal bool StoreKey(byte[] key, string password)
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

        internal bool StoreKey(byte[] key, byte[] KEK)
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

        internal byte[] ReadKeyBytes(string password)
        {
            var keyFileContents = File.ReadAllBytes(KeyFilePath);
            return PasswordBasedEncryptor.Decrypt(keyFileContents, password);
        }

        internal PinnedBytes ReadPinnedKeyBytes(SecureString password)
        {
            var keyFileContents = File.ReadAllBytes(KeyFilePath);
            return PasswordBasedEncryptor.DecryptPinned(keyFileContents, password);
        }

        internal byte[] ReadKeyBytes(byte[] KEK)
        {
            var keyFileContents = File.ReadAllBytes(KeyFilePath);
            return AesTool.Decrypt(KEK, keyFileContents);
        }

        internal bool RemoveKey()
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
        internal byte[] BackupTheKey(string backupFilePath)
        {
            try
            {
                if (!IsKeyStored()) return null;

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

        internal bool RestoreBackup(string path, byte[] backupKey)
        {
            if (IsKeyStored()) return false;

            var fileBytes = File.ReadAllBytes(path);

            var decryptedFileBytes = RandomKeyBasedEncryptor.Decrypt(backupKey, fileBytes);
            if (decryptedFileBytes == null) return false;

            File.WriteAllBytes(KeyFilePath, decryptedFileBytes);

            return true;
        }
    }
}
