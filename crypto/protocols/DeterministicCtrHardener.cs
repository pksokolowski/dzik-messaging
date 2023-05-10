using Dzik.crypto.algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.protocols
{
    internal static class DeterministicCtrHardener
    {
        internal static byte[] ToggleHardening(ProtectedBytes key, byte[] cipherText)
        {
            var nonceStartIndex = cipherText.Length - Constants.CtrNonceLengthBytes;

            var nonce = new byte[Constants.CtrNonceLengthBytes];
            Array.Copy(cipherText, nonceStartIndex, nonce, 0, Constants.CtrNonceLengthBytes);

            var partToEncrypt = new byte[cipherText.Length - Constants.CtrNonceLengthBytes];
            Array.Copy(cipherText, 0, partToEncrypt, 0, partToEncrypt.Length);

            using (var keyPinnedBytes = key.obtainArray())
            {
                var encryptedPart = AesCtrTool.Transform(keyPinnedBytes.bytes, partToEncrypt, nonce);

                var output = new byte[cipherText.Length];
                Array.Copy(encryptedPart, 0, output, 0, encryptedPart.Length);
                Array.Copy(nonce, 0, output, nonceStartIndex, Constants.CtrNonceLengthBytes);

                return output;
            }
        }
    }
}
