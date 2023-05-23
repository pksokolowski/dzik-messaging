using Dzik.common;
using Dzik.crypto.protocols;
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
        private static Func<KeysVault> GetKeysVaultOrNull;

        internal static void Setup(Window window, TextBox input, Action<bool> onHasUnsavedChangesChanged, Func<KeysVault> getKeysVaultOrNull)
        {
            ((App)Application.Current).SessionEnding += DataLossPreventor.OnSessionEnding;
            window.Closing += Window_Closing;

            GetKeysVaultOrNull = getKeysVaultOrNull;
            inputTextBox = input;
            inputTextBox.TextChanged += InputTextBox_TextChanged;
            OnHasUnsavedChangesChanged = onHasUnsavedChangesChanged;
        }

        private static void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HasPotentiallyUnsavedChanges = true;
            SignalUnsavedChangesState(true);
        }

        private static TextBox inputTextBox;
        private static bool HasPotentiallyUnsavedChanges = false;
        private static bool LastNotifiedHasUnsavedChState = false;
        private static Action<bool> OnHasUnsavedChangesChanged;

        internal static void OnDataSaved()
        {
            HasPotentiallyUnsavedChanges = false;
            SignalUnsavedChangesState(false);
        }

        private static void SignalUnsavedChangesState(bool hasUnsavedChanges)
        {
            if (hasUnsavedChanges == LastNotifiedHasUnsavedChState) return;
            LastNotifiedHasUnsavedChState = hasUnsavedChanges;

            OnHasUnsavedChangesChanged(hasUnsavedChanges);
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
                        DraftStorage.Store(inputTextBox, GetKeysVaultOrNull());
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
