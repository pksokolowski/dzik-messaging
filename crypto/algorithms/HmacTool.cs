using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.algorithms
{
    internal class HmacTool
    {
        private const int FirstSignaturePartLen = 15;
        private const int SecondSignaturePartLen = 32 - FirstSignaturePartLen;
        private const int SignatureLen = FirstSignaturePartLen + SecondSignaturePartLen;

        internal static byte[] GenerateKey()
        {
            byte[] secretkey = new byte[Constants.MasterAuthhKeyLenBytes];
            RandomNumberGenerator.Create().GetBytes(secretkey);
            return secretkey;
        }

        internal static byte[] Sign(byte[] authKey, byte[] messageBytes)
        {
            var signatureBytes = ComputeSignature(authKey, messageBytes);
            var signedMessageBytes = CombineSignatureAndMessageBytes(signatureBytes, messageBytes);
            return signedMessageBytes;
        }

        internal static byte[] Verify(byte[] authKey, byte[] signedMessageBytes)
        {
            var (signatureBytes, messageBytes) = SeparateSignatureAndMessageBytes(signedMessageBytes);

            if (!IsSignatureValidFor(authKey, messageBytes, signatureBytes))
            {
                return null;
            }

            return messageBytes;
        }

        internal static bool IsSignatureValidFor(byte[] authKey, byte[] messageBytes, byte[] signatureToCheck)
        {
            if (signatureToCheck.Length != SignatureLen)
            {
                return false;
            }

            var independentSignature = ComputeSignature(authKey, messageBytes);

            if (!StructuralComparisons.StructuralEqualityComparer.Equals(signatureToCheck, independentSignature))
            {
                return false;
            }

            return true;
        }

        internal static byte[] ComputeSignature(byte[] authKey, byte[] messageBytes)
        {
            using (HMACSHA256 hmac = new HMACSHA256(authKey))
            {
                return hmac.ComputeHash(messageBytes);
            }
        }

        private static (byte[], byte[]) SeparateSignatureAndMessageBytes(byte[] signedMessageBytes)
        {
            var lastSignaturePart2LenBytesIndex = signedMessageBytes.Length - SecondSignaturePartLen;

            var signatureBytes = new byte[SignatureLen];
            Array.Copy(signedMessageBytes, 0, signatureBytes, 0, FirstSignaturePartLen);
            Array.Copy(signedMessageBytes, lastSignaturePart2LenBytesIndex, signatureBytes, FirstSignaturePartLen, SecondSignaturePartLen);

            var pureMessageBytes = new byte[signedMessageBytes.Length - SignatureLen];
            Array.Copy(signedMessageBytes, FirstSignaturePartLen, pureMessageBytes, 0, signedMessageBytes.Length - SignatureLen);

            return (signatureBytes, pureMessageBytes);
        }

        private static byte[] CombineSignatureAndMessageBytes(byte[] signatureBytes, byte[] messageBytes)
        {
            var combinedBytes = new byte[signatureBytes.Length + messageBytes.Length];
            Array.Copy(messageBytes, 0, combinedBytes, FirstSignaturePartLen, messageBytes.Length);
            Array.Copy(signatureBytes, 0, combinedBytes, 0, FirstSignaturePartLen);
            var lastSignaturePart2LenBytesIndex = combinedBytes.Length - SecondSignaturePartLen;
            Array.Copy(signatureBytes, FirstSignaturePartLen, combinedBytes, lastSignaturePart2LenBytesIndex, SecondSignaturePartLen);

            return combinedBytes;
        }
    }
}
