using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.algorithms
{
    internal static class AesCbcTool
    {
        internal static byte[] GenerateKey()
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                var keyBytes = aes.Key;

                return aes.Key;
            }
        }

        internal static byte[] Encrypt(byte[] plainText, byte[] Key, PaddingMode paddingMode = PaddingMode.PKCS7)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");

            using (Aes aes = new AesCng() { Mode = CipherMode.CBC, Padding = paddingMode })
            {
                aes.Key = Key;
                byte[] iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    var encrypted = PerformCryptography(plainText, encryptor);

                    var encryptedMessage = new byte[iv.Length + encrypted.Length];
                    Array.Copy(iv, 0, encryptedMessage, 0, iv.Length);
                    Array.Copy(encrypted, 0, encryptedMessage, iv.Length, encrypted.Length);

                    return encryptedMessage;
                }
            }

        }

        internal static byte[] Decrypt(byte[] encryptedMessageBytes, byte[] Key, PaddingMode paddingMode = PaddingMode.PKCS7)
        {
            if (encryptedMessageBytes == null || encryptedMessageBytes.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");


            using (Aes aes = new AesCng() { Mode = CipherMode.CBC, Padding = paddingMode })
            {
                byte[] iv = new byte[aes.IV.Length];
                Array.Copy(encryptedMessageBytes, 0, iv, 0, iv.Length);

                byte[] cipherText = new byte[encryptedMessageBytes.Length - iv.Length];
                Array.Copy(encryptedMessageBytes, iv.Length, cipherText, 0, cipherText.Length);

                aes.Key = Key;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(cipherText, decryptor);
                }
            }
        }

        private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();

                return ms.ToArray();
            }
        }

    }
}
