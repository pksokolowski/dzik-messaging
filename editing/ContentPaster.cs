using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dzik.editing
{
    internal static class ContentPaster
    {
        internal static void PasteInto(TextBox tb, String content)
        {
            var carretPos = tb.SelectionStart;
            tb.Text = tb.Text.Insert(tb.SelectionStart, content);
        }
    }
}
