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

                if (initialSelectionLen == 0)
                {
                    if (isCarretAtBeginningOfLine)
                    {
                        tb.Text = tb.Text.Insert(initialSelectionStart, markerAndSpace);
                        tb.Select(initialSelectionStart + markerAndSpace.Length, 0);
                    }
                    else
                    {
                        tb.Text = tb.Text.Insert(initialSelectionStart, NewLinesMarkerAndSpace);
                        tb.Select(initialSelectionStart + NewLinesMarkerAndSpace.Length, 0);
                    }
                }
                else
                {
                    if (isCarretAtBeginningOfLine)
                    {
                        var selectionText = tb.Text.Substring(initialSelectionStart, initialSelectionLen);
                        tb.Text = tb.Text.Remove(initialSelectionStart, initialSelectionLen);

                        var prepended = LinesPrepender.PrependedRetainingNewLines(selectionText, markerAndSpace, true);
                        tb.Text = tb.Text.Insert(initialSelectionStart, prepended);
                        tb.Select(initialSelectionStart + markerAndSpace.Length, prepended.Length - markerAndSpace.Length);
                    }
                    else
                    {
                        var selectionText = tb.Text.Substring(initialSelectionStart, initialSelectionLen);
                        tb.Text = tb.Text.Remove(initialSelectionStart, initialSelectionLen);

                        var prepended = LinesPrepender.PrependedRetainingNewLines(selectionText, NewLinesMarkerAndSpace, true);
                        tb.Text = tb.Text.Insert(initialSelectionStart, prepended);
                        tb.Select(initialSelectionStart + NewLinesMarkerAndSpace.Length, prepended.Length - NewLinesMarkerAndSpace.Length);
                    }
                }

                tb.Focus();
                tb.EndChange();
            }
            catch (Exception)
            {
                tb.EndChange();
                DialogShower.ShowError("Oznaczanie do szyfrowania nie powiodło się.");
            }
        }
    }
}
