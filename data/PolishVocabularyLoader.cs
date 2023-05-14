using Dzik.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.data
{
    internal static class PolishVocabularyLoader
    {
        internal static string[] GetVocabulary()
        {
            return Resources.PolishVocabulary.Replace("\r\n", "\n").Split('\n');
        }
    }
}
