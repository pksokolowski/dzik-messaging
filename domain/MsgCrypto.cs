using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.domain
{
    internal interface Encryptor
    {
        String Encrypt(string plainText);
    }

    internal interface Decryptor
    {
        String Decrypt(string cipherText);
    }

    internal class MockEncryptor : Encryptor
    {
        public string Encrypt(string plainText)
        {
            return plainText.ToUpper();
        }
    }

    internal class MockDecryptor : Decryptor
    {
        public string Decrypt(string plainText)
        {
            return plainText.ToLower();
        }
    }
}
