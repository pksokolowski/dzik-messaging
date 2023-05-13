
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
                output[i] = (byte)(input[i] - GetByteShift(input[i]));
            }

            return output;
        }

        internal static int GetUTF16Shift(byte b)
        {
            if (b < SECOND_RANGE_BYTE_THRESHOLD) return FIRST_RANGE_UTF_START;
            return SECOND_RANGE_CORRECTED;
        }

        internal static int GetByteShift(int utf16)
        {
            if (utf16 >= SECOND_RANGE_CORRECTED) return SECOND_RANGE_CORRECTED;
            return FIRST_RANGE_UTF_START;
        }
        
        internal const int FIRST_RANGE_UTF_START = 249;
        internal const int SECOND_RANGE_UTF_START = 768;
        internal const int SECOND_RANGE_BYTE_THRESHOLD = 155;

        internal const int SECOND_RANGE_CORRECTED = SECOND_RANGE_UTF_START - SECOND_RANGE_BYTE_THRESHOLD;
    }
}

