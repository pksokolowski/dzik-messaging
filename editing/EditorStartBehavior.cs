using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dzik.editing
{
    internal static class EditorStartBehavior
    {
        internal static void RestoreDraft(TextBox input)
        {
            var draft = DraftStorage.ReadDraft();
            if (draft == null) return;

            input.Text = draft;
            DataLossPreventor.OnDataSaved();
        }   
    }
}
