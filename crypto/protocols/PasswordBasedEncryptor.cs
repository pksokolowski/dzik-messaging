using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Dzik.crypto.algorithms;

namespace Dzik.crypto.protocols
{
    internal class PasswordBasedEncryptor
    {
        private const int saltBytesLen = 17;

        internal static byte[] Encrypt(byte[] data, string password)
        {
            var passwordUft8Bytes = Encoding.UTF8.GetBytes(password);

            byte[] saltBytes = new byte[saltBytesLen];
            RandomNumberGenerator.Create().GetBytes(saltBytes);

            var expandedPassword = ExpandTo256Bits(passwordUft8Bytes, saltBytes);
            var encryptedData = AesTool.Encrypt(expandedPassword, data);

            var saltAndCiphertext = new byte[saltBytes.Length + encryptedData.Length];
            Array.Copy(saltBytes, 0, saltAndCiphertext, 0, saltBytes.Length);
            Array.Copy(encryptedData, 0, saltAndCiphertext, saltBytes.Length, encryptedData.Length);

            return saltAndCiphertext;
        }

        internal static byte[] Decrypt(byte[] encryptedDataWithSalt, string password)
        {
            try
            {
                var passwordUft8Bytes = Encoding.UTF8.GetBytes(password);
                var components = DecomposeEncryptedData(encryptedDataWithSalt);
                var expandedPassword = ExpandTo256Bits(passwordUft8Bytes, components.Salt);
                var data = AesTool.Decrypt(expandedPassword, components.CipherText);

                return data;
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static PinnedBytes DecryptPinned(byte[] encryptedDataWithSalt, SecureString password)
        {
            try
            {
                var passwordBytesPlain = ProtectedBytes.getArrayFromSecureString(password);
                if (passwordBytesPlain == null) return null;

                var pinnedUtf8PasswordBytes = new PinnedBytes(Encoding.Convert(Encoding.Unicode, Encoding.UTF8, passwordBytesPlain.bytes));
                passwordBytesPlain.Dispose();

                var components = DecomposeEncryptedData(encryptedDataWithSalt);

                var expandedPassword = new PinnedBytes(ExpandTo256Bits(pinnedUtf8PasswordBytes.bytes, components.Salt));
                pinnedUtf8PasswordBytes.Dispose();

                var data = AesTool.DecryptPinned(expandedPassword.bytes, components.CipherText);
                expandedPassword.Dispose();

                return data;

            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static byte[] ExpandTo256Bits(byte[] utf8PasswordBytes, byte[] salt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(utf8PasswordBytes, salt, Constants.KeyDerivationIterationsCount, HashAlgorithmName.SHA256);
            var derivedKeyBytes = pbkdf2.GetBytes(Constants.AesKeysLenBytes);
            pbkdf2.Dispose();

            if (derivedKeyBytes.Length != 32) throw new Exception("Derived non-32 bytes with SHA256 and PBKDF2. Derived key' length must match the hash function output length!");

            return derivedKeyBytes;
        }

        private static ComponentsOfEncryptedData DecomposeEncryptedData(byte[] encryptedData)
        {
            byte[] salt = new byte[saltBytesLen];
            byte[] ciphertext = new byte[encryptedData.Length - salt.Length];
            Array.Copy(encryptedData, 0, salt, 0, salt.Length);
            Array.Copy(encryptedData, salt.Length, ciphertext, 0, ciphertext.Length);

            return new ComponentsOfEncryptedData(salt, ciphertext);
        }

        private class ComponentsOfEncryptedData
        {
            public byte[] Salt;
            public byte[] CipherText;

            public ComponentsOfEncryptedData(byte[] salt, byte[] cipherText)
            {
                Salt = salt;
                CipherText = cipherText;
            }
        }
    }
}
