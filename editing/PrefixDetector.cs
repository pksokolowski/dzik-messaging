using Dzik.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dzik.editing
{
    internal class PrefixDetector
    {
        internal static bool HasPrefix(TextBox tb, string prefix)
        {
            try
            {
                var isCarretAtBeginningOfLine = LinesPrepender.IsCarretAtTheBeginningOfLine(tb);
                var initialSelectionStart = tb.SelectionStart;
                var initialSelectionLen = tb.SelectionLength;                

                var beginningOfFirstSelectedLine =  LinesPrepender.GetLineStartIndex(tb);
                var extendedSelectionLen = initialSelectionLen + (initialSelectionStart - beginningOfFirstSelectedLine);

                var selectionText = tb.Text.Substring(beginningOfFirstSelectedLine, extendedSelectionLen);

                if (selectionText.StartsWith(prefix) || selectionText.Contains($"\n{prefix}")) return true;

                return false;
            }
            catch (Exception we)
            {
                DialogShower.ShowError("Błąd analizy treści inputu w poszukiwaniu znacznika");
                return false;
            }
        }
    }
}
