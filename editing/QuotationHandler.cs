using Dzik.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dzik.editing
{
    internal static class QuotationHandler
    {
        internal static void Handle(TextBox tb, string content)
        {
            try
            {
                var initialSelectionStart = tb.SelectionStart;

                var quoted = LinesPrepender.Prepended(content, "> ", true);
                if (!LinesPrepender.IsCarretAtTheBeginningOfLine(tb)) quoted = "\n\n" + quoted;

                // add line breaks after, for convenience
                quoted += "\n\n";
                ContentPaster.PasteInto(tb, quoted);
                tb.SelectionStart = initialSelectionStart + quoted.Length;
                tb.Focus();
            }
            catch (Exception)
            {
                DialogShower.ShowError("Dodawanie cytatu nie powiodło się.");
            }
        }
    }
}
