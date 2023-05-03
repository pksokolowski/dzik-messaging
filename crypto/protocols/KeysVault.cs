using Dzik.crypto.algorithms;
using Dzik.crypto.utils;
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
            var ciphertextString = Base64PL.StringFromBytes(cipherTextBytes);

            return ciphertextString;
        }


        public string Decrypt(string cipherText)
        {
            var cipherTextBytes = Base64PL.BytesFromString(cipherText);
            var plainTextBytes = AesCbcTool.Decrypt(cipherTextBytes, key);
            var plainTextString = Encoding.UTF8.GetString(plainTextBytes);

            return plainTextString;
        }
    }
}
