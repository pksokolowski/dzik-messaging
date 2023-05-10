using Dzik.crypto.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.protocols
{
    /// <summary>
    /// Packs and unpacks master keys, can create a KeysVault from byte[] with keys data
    /// </summary>
    internal static class MasterKeysPacker
    {
        private const int keyLenBytes = Constants.MasterSymmetricKeyLenBytes;
        private const int authKeyLenBytes = Constants.MasterAuthhKeyLenBytes;
        private const int allKeysBundleLen = 5 * keyLenBytes + authKeyLenBytes;

        internal static byte[] PackKeys(byte[] masterA, byte[] masterB, byte[] masterC, byte[] masterD, byte[] masterE, byte[] hmacKey)
        {
            if (masterA.Length != keyLenBytes || masterB.Length != keyLenBytes || masterC.Length != keyLenBytes || masterD.Length != keyLenBytes || masterE.Length != keyLenBytes || hmacKey.Length != authKeyLenBytes)
            {
                throw new Exception($"Incorrect key length! Expected {keyLenBytes * 8} but got a different length on at least one of the keys");
            }

            var allKeys = new byte[masterA.Length + masterB.Length + masterC.Length + masterD.Length + masterE.Length + hmacKey.Length];

            Array.Copy(masterA, 0, allKeys, 0, masterA.Length);
            Array.Copy(masterB, 0, allKeys, keyLenBytes, masterB.Length);
            Array.Copy(masterC, 0, allKeys, 2 * keyLenBytes, masterC.Length);
            Array.Copy(masterD, 0, allKeys, 2 * keyLenBytes, masterD.Length);
            Array.Copy(masterE, 0, allKeys, 2 * keyLenBytes, masterE.Length);
            Array.Copy(hmacKey, 0, allKeys, 3 * keyLenBytes, hmacKey.Length);

            return allKeys;
        }

        internal static KeysVault UnpackKeys(byte[] allKeys)
        {
            // pinning here is done for compatibility alone.           
            using (var pinnedAllKeys = new PinnedBytes(allKeys.Length))
            {
                // don't pin the provided allBytes array, it would erase its contents after the using block.
                Array.Copy(allKeys, 0, pinnedAllKeys.bytes, 0, allKeys.Length);
                return UnpackKeys(pinnedAllKeys);
            }
        }

        internal static KeysVault UnpackKeys(PinnedBytes allKeys)
        {
            var (masterA, masterB, masterC, masterD, masterE, hmacKey) = UnpackKeysIntoNArrays(allKeys);

            try
            {
                return new KeysVault(masterA, masterB, masterC, masterD, masterE, hmacKey);
            }
            catch (Exception) { }
            finally
            {
                masterA.Dispose();
                masterB.Dispose();
                masterC.Dispose();
                masterD.Dispose();
                masterE.Dispose();
                hmacKey.Dispose();
            }

            return null;
        }

        internal static string GetStringRepresentation(byte[] allKeys)
        {
            // pinning here is done for compatibility alone.
            var pinnedAllKeys = new PinnedBytes(allKeys.Length);
            // work on a copy of the allBytes array though, otherwise the provided array would be cleared after disposal of the PinnedBytes
            Array.Copy(allKeys, 0, pinnedAllKeys.bytes, 0, allKeys.Length);

            var (masterA, masterB, masterC, masterD, masterE, hmacKey) = UnpackKeysIntoNArrays(pinnedAllKeys);

            var builder = new StringBuilder();
            builder.AppendLine(ByteArrayHexStringConverters.MakePresentableStringOf(masterA.bytes));
            builder.AppendLine(ByteArrayHexStringConverters.MakePresentableStringOf(masterB.bytes));
            builder.AppendLine(ByteArrayHexStringConverters.MakePresentableStringOf(masterC.bytes));
            builder.AppendLine(ByteArrayHexStringConverters.MakePresentableStringOf(masterD.bytes));
            builder.AppendLine(ByteArrayHexStringConverters.MakePresentableStringOf(masterE.bytes));
            builder.AppendLine();
            builder.AppendLine(ByteArrayHexStringConverters.MakePresentableStringOf(hmacKey.bytes));

            masterA.Dispose();
            masterB.Dispose();
            masterC.Dispose();
            hmacKey.Dispose();

            // pinning here is dones just for compatibility
            pinnedAllKeys.Dispose();

            return builder.ToString();
        }

        /// <summary>
        /// Warning: returns disposables with sensitive data!
        /// </summary>
        /// <exception cref="Exception">Throws when len of bytes provided is not as expected</exception>
        private static (PinnedBytes, PinnedBytes, PinnedBytes, PinnedBytes, PinnedBytes, PinnedBytes) UnpackKeysIntoNArrays(PinnedBytes allKeys)
        {
            if (allKeys.bytes.Length != allKeysBundleLen)
            {
                var actualLen = allKeys.bytes.Length;
                throw new Exception($"All keys byte[] was expected to be of length = {allKeysBundleLen} bytes, but was in fact of length = {actualLen}");
            }

            var masterA = new PinnedBytes(keyLenBytes);
            var masterB = new PinnedBytes(keyLenBytes);
            var masterC = new PinnedBytes(keyLenBytes);
            var masterD = new PinnedBytes(keyLenBytes);
            var masterE = new PinnedBytes(keyLenBytes);
            var hmacKey = new PinnedBytes(authKeyLenBytes);

            Array.Copy(allKeys.bytes, 0, masterA.bytes, 0, keyLenBytes);
            Array.Copy(allKeys.bytes, keyLenBytes, masterB.bytes, 0, keyLenBytes);
            Array.Copy(allKeys.bytes, 2 * keyLenBytes, masterC.bytes, 0, keyLenBytes);
            Array.Copy(allKeys.bytes, 3 * keyLenBytes, masterD.bytes, 0, keyLenBytes);
            Array.Copy(allKeys.bytes, 4 * keyLenBytes, masterE.bytes, 0, keyLenBytes);
            Array.Copy(allKeys.bytes, 5 * keyLenBytes, hmacKey.bytes, 0, authKeyLenBytes);

            return (masterA, masterB, masterC, masterD, masterE, hmacKey);
        }
    }
}
