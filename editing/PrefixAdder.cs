using Dzik.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dzik.editing
{
    internal class PrefixAdder
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

                var markerAndSpace = $"{prefix} ";
                var NewLinesMarkerAndSpace = $"\n\n{prefix} ";


                var selectionText = tb.Text.Substring(beginningOfFirstSelectedLine, extendedSelectionLen);
                tb.Text = tb.Text.Remove(beginningOfFirstSelectedLine, extendedSelectionLen);

                var prepended = LinesPrepender.PrependedRetainingNewLines(selectionText, markerAndSpace, true);
                tb.Text = tb.Text.Insert(beginningOfFirstSelectedLine, prepended);            
                tb.Select(beginningOfFirstSelectedLine + prepended.Length, 0);


                tb.Focus();
                tb.EndChange();
            }
            catch (Exception)
            {
                tb.EndChange();
                DialogShower.ShowError("Oznaczanie tekstu markerem nie powiodło się");
            }
        }
    }
}
