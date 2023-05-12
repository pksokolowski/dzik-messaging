using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.utils
{
    internal class TagUtil
    {
        internal static bool StartsWithTag(string TAG, string content)
        {
            if (content.Length < TAG.Length) return false;
            if (content.Substring(0, TAG.Length) == TAG) return true;
            return false;
        }
    }
}
