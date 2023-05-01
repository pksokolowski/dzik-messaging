using Dzik.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dzik.editing
{
    internal static class DataLossPreventor
    {
        internal static void Setup(Window window, TextBox input)
        {
            ((App)Application.Current).SessionEnding += DataLossPreventor.OnSessionEnding;
            window.Closing += Window_Closing;

            inputTextBox = input;
            inputTextBox.TextChanged += InputTextBox_TextChanged;
        }

        private static void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HasPotentiallyUnsavedChanges = true;
        }

        private static TextBox inputTextBox;
        private static bool HasPotentiallyUnsavedChanges = false;

        internal static void OnDataSaved()
        {
            HasPotentiallyUnsavedChanges = false;
        }

        private static void OnSessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            if (!HasPotentiallyUnsavedChanges) return;
            e.Cancel = true;
        }

        private static void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!HasPotentiallyUnsavedChanges) return;

            var usersChoice = DialogShower.ShowSaveExitCancelDialog("Edytor Dzika", "Czy chcesz zapisać zmiany?");
            switch (usersChoice)
            {
                case DialogShower.UsersChoice.yes:
                    try
                    {
                        DraftStorage.Store(inputTextBox);
                    }
                    catch
                    {
                        e.Cancel = true;
                    }
                    break;
                case DialogShower.UsersChoice.no:
                    break;
                case DialogShower.UsersChoice.cancel:
                    e.Cancel = true;
                    break;
            }
        }

    }
}
