using Dzik.crypto.algorithms;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.protocols
{
    internal static class RandomKeyBasedEncryptor
    {
        /// <returns>Encrypted data (or null, if encryption fails) and the key used to encrypt it</returns>
        internal static (byte[], byte[]) Encrypt(byte[] data)
        {
            var key = new byte[Constants.AesKeysLenBytes];
            RandomNumberGenerator.Create().GetBytes(key);

            var encryptedData = AesTool.Encrypt(key, data);

            return (encryptedData, key);
        }

        internal static byte[] Decrypt(byte[] key, byte[] encryptedData) => AesTool.Decrypt(key, encryptedData);
    }
}
