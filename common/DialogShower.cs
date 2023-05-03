using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dzik.common
{
    internal static class DialogShower
    {
        internal static void ShowDialog(string title, string message, MessageBoxImage icon)
        {
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBox.Show(message, title, button, icon, MessageBoxResult.OK);
        }

        internal static void ShowDangerousWarning(string title, string message) => ShowDialog(title, message, MessageBoxImage.Warning);
        internal static void ShowError(string message) => ShowDialog("Wystąpił błąd", message, MessageBoxImage.Error);
        internal static void ShowInfo(string title, string message) => ShowDialog(title, message, MessageBoxImage.Information);

        internal static UsersChoice ShowSaveExitCancelDialog(string title, string message)
        {
            var choice = MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

            switch (choice)
            {
                case MessageBoxResult.Yes: return UsersChoice.yes;
                case MessageBoxResult.No: return UsersChoice.no;
                case MessageBoxResult.Cancel: return UsersChoice.cancel;
                default: return UsersChoice.cancel;
            }
        }

        internal enum UsersChoice { yes, no, cancel }
    }
}
