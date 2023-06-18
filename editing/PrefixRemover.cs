using Dzik.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dzik.editing
{
    internal class PrefixRemover
    {
        internal static void Handle(TextBox tb, string prefix)
        {
            tb.BeginChange();
            try
            {
                var isCarretAtBeginningOfLine = LinesPrepender.IsCarretAtTheBeginningOfLine(tb);
                var initialSelectionStart = tb.SelectionStart;
                var initialSelectionLen = tb.SelectionLength;

                var beginningOfFirstSelectedLine = LinesPrepender.GetLineStartIndex(tb);
                var extendedSelectionLen = initialSelectionLen + (initialSelectionStart - beginningOfFirstSelectedLine);

                var selectionText = tb.Text.Substring(beginningOfFirstSelectedLine, extendedSelectionLen);
                tb.Text = tb.Text.Remove(beginningOfFirstSelectedLine, extendedSelectionLen);

                var withReplacedPrefixes = selectionText.Replace($"\n{prefix} ", "\n").Replace($"\n{prefix}", "\n").TrimStart();
                if (withReplacedPrefixes.StartsWith(prefix))
                {
                    withReplacedPrefixes = withReplacedPrefixes.Substring(prefix.Length).TrimStart();
                }

                tb.Text = tb.Text.Insert(beginningOfFirstSelectedLine, withReplacedPrefixes);
                tb.Select(beginningOfFirstSelectedLine + withReplacedPrefixes.Length, 0);


                tb.Focus();
                tb.EndChange();
            }
            catch (Exception)
            {
                tb.EndChange();
                DialogShower.ShowError("Usuwanie znacznika nie powiodło się.");
            }
        }
    }
}
