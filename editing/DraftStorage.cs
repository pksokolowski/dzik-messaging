using Dzik.crypto.protocols;
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
        internal static void Store(TextBox input, KeysVault vaultOrNull)
        {
            if (vaultOrNull == null)
            {
                var content = input.Text;
                StorageManager.SaveDraft(content);
            }
            else
            {
                var bytes = Encoding.UTF8.GetBytes(input.Text);
                var ciphertext = vaultOrNull.EncryptAndSign(bytes);
                StorageManager.SaveEncryptedDraft(ciphertext);
            }

            DataLossPreventor.OnDataSaved();
        }

        internal static String ReadDraft(KeysVault vaultOrNull)
        {
            try
            {
                if (vaultOrNull == null)
                {
                    var content = StorageManager.ReadDraft();
                    return content;
                }
                else
                {
                    var draftBytes = StorageManager.ReadEncryptedDraft();
                    var decryptionResult = vaultOrNull.VerifyAndDecrypt(draftBytes);
                    return Encoding.UTF8.GetString(decryptionResult.plainText);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
