using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.utils
{
    public static class ByteArrayModificationUtils
    {
        /// <summary>
        /// Adds a prefix to a byte array, the prefix can then be taken off from it with StripPrefix method.
        /// Handles up to 255 utf8 bytes long prefixes.
        /// </summary>
        /// <param name="inputBytes">byte array to prepend with a prefix information.</param>
        /// <param name="prefix">the prefix string to store in the resulting byte[]</param>
        /// <param name="maxAllowedBytesLengthOfPrefix"> a number up to 255</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static byte[] PrependedWith(this byte[] inputBytes, string prefix, int maxAllowedBytesLengthOfPrefix = 255)
        {
            if (maxAllowedBytesLengthOfPrefix > 255) throw new Exception("Cannot set maxAllowedBytesLengthOfPrefix to more than 255");
            if (maxAllowedBytesLengthOfPrefix == 0) throw new Exception("Cannot set maxAllowedBytesLengthOfPrefix to 0");

            var prefixBytes = Encoding.UTF8.GetBytes(prefix);
            if (prefixBytes.Length > maxAllowedBytesLengthOfPrefix) throw new Exception("Prefix length exceeds max allowed prefix length!");

            var prefixLenByte = (byte)prefixBytes.Length;

            var output = new byte[1 + prefixBytes.Length + inputBytes.Length];
            output[0] = prefixLenByte;
            Array.Copy(prefixBytes, 0, output, 1, prefixBytes.Length);
            Array.Copy(inputBytes, 0, output, 1 + prefixBytes.Length, inputBytes.Length);

            return output;
        }

        /// <summary>
        /// Removes a prefix from byte array.
        /// </summary>
        /// <param name="inputBytes"></param>
        /// <returns>the prefix string and a byte[] without prefix data</returns>
        internal static (string, byte[]) StripPrefix(this byte[] inputBytes)
        {
            var prefixLen = (int)inputBytes[0];

            var prefixBytes = new byte[prefixLen];
            Array.Copy(inputBytes, 1, prefixBytes, 0, prefixLen);
            var prefix = Encoding.UTF8.GetString(prefixBytes);

            var stripped = new byte[inputBytes.Length - (1 + prefixLen)];
            Array.Copy(inputBytes, 1 + prefixLen, stripped, 0, stripped.Length);

            return (prefix, stripped);
        }

        internal static byte[] PrependWithConstantLenPrefix(byte[] prefix, byte[] data)
        {
            if (prefix.Length == 0 || data.Length == 0) return null;

            var output = new byte[prefix.Length + data.Length];
            Array.Copy(prefix, 0, output, 0, prefix.Length);
            Array.Copy(data, 0, output, prefix.Length, data.Length);

            return output;
        }

        internal static StripConstantLenPrefixResult StripConstantLenPrefix(int prefixLen, byte[] prefixedData)
        {
            if (prefixedData.Length < prefixLen + 1) return null;

            var prefix = new byte[prefixLen];
            var data = new byte[prefixedData.Length - prefixLen];

            Array.Copy(prefixedData, 0, prefix, 0, prefixLen);
            Array.Copy(prefixedData, prefixLen, data, 0, data.Length);

            return new StripConstantLenPrefixResult(prefix, data);
        }
    }

    internal class StripConstantLenPrefixResult
    {
        public byte[] prefix;
        public byte[] data;

        public StripConstantLenPrefixResult(byte[] prefix, byte[] data)
        {
            this.prefix = prefix;
            this.data = data;
        }
    }
}
