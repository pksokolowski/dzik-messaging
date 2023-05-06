using Dzik.crypto.algorithms;
using Dzik.crypto.protocols;
using Dzik.crypto.utils;
using Dzik.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.api
{
    internal class MsgCryptoTool : Encryptor, Decryptor
    {
        private KeysVault keysVault;

        internal MsgCryptoTool(KeysVault keysVault)
        {
            this.keysVault = keysVault;
        }

        public string Encrypt(string plainText)
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherTextBytes = keysVault.EncryptAndSign(plaintextBytes);
            var ciphertextString = Base64PL.StringFromBytes(cipherTextBytes);

            return ciphertextString;
        }


        public DecryptedMsg Decrypt(string cipherText)
        {
            var cipherTextBytes = Base64PL.BytesFromString(cipherText);
            var verifyAndDecryptResult = keysVault.VerifyAndDecrypt(cipherTextBytes);
            var plainTextString = Encoding.UTF8.GetString(verifyAndDecryptResult.plainText);

            return new DecryptedMsg(plainTextString, verifyAndDecryptResult.daysSinceEncryption);
        }
    }
}
