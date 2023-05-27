using Dzik.crypto.protocols;
using Dzik.data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.letter
{
    internal class XamlMessageDraftStorage
    {
        internal static bool HasStoredDraft()
        {
            return LettersStorageManager.IsDraftStored();
        }

        internal static bool StoreDraftState(byte[] inboundMessage, byte[] draftMessage, KeysVault vault)
        {
            // potentially store inbound message, this should only happen once per new inbound message
            if (inboundMessage != null)
            {
                var encryptedInboundMessage = vault.EncryptAndSign(inboundMessage);
                if (encryptedInboundMessage == null) return false;
                try
                {
                    LettersStorageManager.StoreInboundMessage(encryptedInboundMessage);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            // Store draft
            var encryptedDraftMessage = vault.EncryptAndSign(draftMessage);
            if (encryptedDraftMessage == null) return false;
            try
            {
                LettersStorageManager.StoreDraftMessage(encryptedDraftMessage);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Loads stored draft data
        /// </summary>
        /// <param name="vault">Vault containing up to date keys and providing cryptographic protocols impl</param>
        /// <returns>byte[] with inbound message decrypted, and byte[] with decrypted bytes of the draft message</returns>
        internal static (byte[], byte[]) LoadDraftState(KeysVault vault)
        {
            try
            {
                var encryptedInboundMessage = LettersStorageManager.ReadInboundMessage();
                var encryptedDraftMessage = LettersStorageManager.ReadDraftMessage();

                var decryptedInboundMessage = vault.VerifyAndDecrypt(encryptedInboundMessage);
                var decryptedDraftMessage = vault.VerifyAndDecrypt(encryptedDraftMessage);

                return (decryptedInboundMessage?.plainText, decryptedDraftMessage?.plainText);
            }
            catch (Exception)
            {
                return (null, null);
            }
        }

        internal static bool EraseDraftData()
        {
            try
            {
                LettersStorageManager.DeleteInbound();
                LettersStorageManager.DeleteDraft();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
