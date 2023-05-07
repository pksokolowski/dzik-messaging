using Dzik.common;
using Dzik.crypto.algorithms;
using Dzik.data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.protocols
{
    internal static class DzikKeyAgreement
    {
        internal static async Task Initialize(Action<KeysVault> onNewKeysGenerated)
        {
            var challenge = StorageManager.ReadKeyAgreementChallengeOrNull();
            var privateKey = StorageManager.ReadKeyAgreementPrivateKeyOrNull();
            // if has nothing, generate private keys and challenge, then return
            if (challenge == null && privateKey == null)
            {           
                var keyPair = await Task.Run(() => RsaTool.GenerateKeyPair());
                var symKey = AesTool.GenerateKey();

                var challengeBytes = KeyAgreementPacker.PackChallenge(keyPair.publicKey, symKey);
                StorageManager.WriteKeyAgreementChallenge(challengeBytes);

                DialogShower.ShowInfo("Wygenerowano challenge, przekaż plik dzik-data/challenge-Share drugiej osobie");

                return;
            }

            // if has challenge, and no private keys, generate response, notify user and return;
            if (challenge == null && privateKey != null)
            {
                var newKeys = MasterKeysGenerator.GenerateMasterKeys();
                StorageManager.WriteMasterKeys(newKeys);

                var keysFromStorage = StorageManager.ReadMasterKeys();

                if (!StructuralComparisons.StructuralEqualityComparer.Equals(keysFromStorage.bytes, newKeys))
                {
                    throw new Exception("Saved new keys are not the same as the keys read right after!");
                }

                keysFromStorage.Dispose();
                var keysVault = MasterKeysPacker.UnpackKeys(newKeys);               

                onNewKeysGenerated(keysVault);
                return;
            }

            // if has challenge and private keys, do nothing here, will handle the response when it arrives, in a separate method.
        }

        internal static void AcceptResponse(byte[] response)
        {
            // if doesnt have private keys, throw exception

            // decrypt response and save master secrets.
        }
    }
}
