using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.utils
{
    internal static class ByteArrayHexStringConverters
    {
        /// <summary>
        /// Parses hex string into byte[]
        /// </summary>
        /// <param name="hexInput"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(this string hexInput)
        {
            var sanitizedHexInput = hexInput.Replace("\n", "").Replace("\r", "").Replace(" ", "").Trim();
            byte[] bytes = new byte[sanitizedHexInput.Length / 2];

            for (int i = 0; i < sanitizedHexInput.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(sanitizedHexInput.Substring(i, 2), 16);

            return bytes;
        }

        public static string ToHexString(this byte[] inputBytes)
        {
            return BitConverter.ToString(inputBytes).Replace("-", string.Empty);
        }

        public static string ToPrettyHexString(this byte[] inputBytes)
        {
            return MakePresentableStringOf(inputBytes);
        }

        public static string MakeHexString(byte[] inputBytes)
        {
            return BitConverter.ToString(inputBytes).Replace("-", string.Empty);
        }

        public static string MakePresentableStringOf(byte[] keyBytes)
        {
            var builder = new StringBuilder();
            var hexString = keyBytes.ToHexString();

            for (int i = 0; i < hexString.Length; i++)
            {
                if (i > 2 && i % 4 == 0) builder.Append(' ');
                builder.Append(hexString[i]);
            }

            return builder.ToString();
        }
    }
}
