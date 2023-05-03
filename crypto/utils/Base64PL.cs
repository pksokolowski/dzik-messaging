using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.utils
{
    internal static class Base64PL
    {
        internal static string StringFromBytes(byte[] bytes)
        {
            var base64String = Convert.ToBase64String(bytes);
            return TogglePolishness(base64String);
        }

        internal static byte[] BytesFromString(string basePLString)
        {
            var base64String = TogglePolishness(basePLString);
            return Convert.FromBase64String(base64String);
        }

        private static string TogglePolishness(string baseString)
        {
            var builder = new StringBuilder();
            foreach (char c in baseString)
            {
                switch (c)
                {
                    // from base64 to PL
                    case '+': builder.Append('ó'); break;
                    case '/': builder.Append('ć'); break;
                    case '=': builder.Append('ą'); break;
                    // from PL to base64
                    case 'ó': builder.Append('+'); break;
                    case 'ć': builder.Append('/'); break;
                    case 'ą': builder.Append('='); break;
                    // other characters, no change
                    default: builder.Append(c); break;
                }
            }
            return builder.ToString();
        }
    }
}
