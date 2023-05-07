using Dzik.data;
using Dzik.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.api
{
    internal class KeyAgreementResponseProviderImpl : KeyExchangeResponseProvider
    {
        public string GetKeyExchangeResponseOrNull()
        {
            return StorageManager.ReadKeyAgreementResponse();
        }
    }
}
