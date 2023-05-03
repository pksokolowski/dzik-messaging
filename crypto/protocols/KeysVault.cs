using Dzik.crypto.algorithms;
using Dzik.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.protocols
{
    internal class KeysVault : Encryptor, Decryptor
    {
        internal byte[] key = AesCbcTool.GenerateKey();


        public string Encrypt(string plainText)
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherTextBytes = AesCbcTool.Encrypt(plaintextBytes, key);
            var ciphertextString = Convert.ToBase64String(cipherTextBytes);

            return ciphertextString;
        }


        public string Decrypt(string cipherText)
        {
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            var plainTextBytes = AesCbcTool.Decrypt(cipherTextBytes, key);
            var plainTextString = Encoding.UTF8.GetString(plainTextBytes);

            return plainTextString;
        }
    }
}
