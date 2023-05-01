using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dzik.editing
{
    internal static class LinesPrepender
    {
        internal static String Prepended(String content, String newLinePrefix, bool forcePrefixAtTheBeginning)
        {
            var withReplacedNewLines = content.Replace("\n", newLinePrefix);

            if (forcePrefixAtTheBeginning)
            {
                return newLinePrefix + withReplacedNewLines;
            }
            else
            {
                return withReplacedNewLines;
            }
        }

        internal static bool IsCarretAtTheBeginningOfLine(TextBox input)
        {
            var cStart = input.SelectionStart;      
            return cStart == 0 || input.Text[cStart - 1] == '\n';
        }

        internal static void PrependSelection(TextBox input, String newLinePrefix)
        {
            var cStart = input.SelectionStart;
            var cEnd = cStart + input.SelectionLength;
            var content = input.Text.Substring(cStart, input.SelectionLength);

            if (cStart == 0 || input.Text[cStart - 1] == '\n')
            {
                content = newLinePrefix + content;
            }
            else
            {
                content = '\n' + content;
            }

            var prefixed = Prepended(content, newLinePrefix, false);

            input.Text = input.Text.Remove(cStart, cEnd).Insert(cStart, content);
        }
    }
}
