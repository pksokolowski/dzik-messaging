using Dzik.crypto.protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.domain
{
    internal interface Encryptor
    {
        /// <summary>
        /// returns basePL ciphertext.
        /// </summary>
        String Encrypt(string plainText);
    }

    internal interface Decryptor
    {
        DecryptedMsg Decrypt(string basePlCiphertext);
    }

    internal interface KeyExchangeResponseProvider
    {
        string GetKeyExchangeResponseOrNull();
    }

    class DecryptedMsg
    {
        public string plainText;
        public long daysSinceEncryption;

        public DecryptedMsg(string plainText, long daysSinceEncryption)
        {
            this.plainText = plainText;
            this.daysSinceEncryption = daysSinceEncryption;
        }
    }
}
