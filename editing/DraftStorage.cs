using Dzik.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dzik.editing
{
    internal class DraftStorage
    {
        internal static void Store(TextBox input)
        {
            var content = input.Text;
            StorageManager.SaveDraft(content);

            DataLossPreventor.OnDataSaved();
        }

        internal static String ReadDraft()
        {
            try
            {
                var content = StorageManager.ReadDraft();
                return content;
            }
            catch
            {
                return null;
            }
        }
    }
}
