using Dzik.common;
using Dzik.crypto.protocols;
using Dzik.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dzik.editing
{
    internal class KeyAgreementResponseHandler
    {
        internal static bool Handle(string content, KeysVault keysVault, Action<KeysVault> onKeysReceivedInExchange)
        {
            try
            {
                if (keysVault != null) return false;
                if (!content.StartsWith(Constants.MARKER_KEY_EXCHANGE_RESPONSE_TO_INTERPRETE)) return false;

                DzikKeyAgreement.AcceptResponse(content, onKeysReceivedInExchange);

                DialogShower.ShowInfo("Accepted key exchange response");

                return true;
            }
            catch (Exception)
            {
                DialogShower.ShowError("Nie udało się przyjąć odpowiedzi wymiany kluczy.");
                return true;
            }
        }
    }
}
