
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.utils
{
    internal static class Base256
    {
        internal static string StringFromBytes(byte[] bytes)
        {
            var builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                var c = (char)(b + GetUTF16Shift(b));
                builder.Append(c);
            }
            return builder.ToString();
        }

        internal static byte[] BytesFromString(string base256String)
        {
            var input = base256String.ToCharArray();
            var output = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                output[i] = (byte)(((int)input[i]) - GetByteShift(input[i]));
            }

            return output;
        }

        internal static int GetUTF16Shift(byte b)
        {
            if (b < 198) return 249;
            return 603;
        }

        internal static int GetByteShift(int utf16)
        {
            if (utf16 >= 603) return 603;
            return 249;
        }
    }
}

