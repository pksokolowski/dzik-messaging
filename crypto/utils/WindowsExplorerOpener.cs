using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.utils
{
    internal static class WindowsExplorerOpener
    {
        internal static void ShowFileInExplorer(string relativePath)
        {
            var fullPath = System.IO.Path.GetFullPath(relativePath);
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", fullPath));
        }
    }
}
