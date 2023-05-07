using Dzik.common;
using Dzik.crypto.algorithms;
using Dzik.crypto.utils;
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
        internal static async Task Initialize(Action<KeysVault> onNewKeysGenerated, Action onKeyExchangeResponseReady)
        {
            var challenge = StorageManager.ReadKeyAgreementChallengeOrNull();
            var privateKey = StorageManager.ReadKeyAgreementPrivateKeyOrNull();
            // if has nothing, generate private keys and challenge, then return
            if (challenge == null && privateKey == null)
            {
                DialogShower.ShowInfo("Rozpoczęto generowanie challengu, nie wyłączaj apki");

                var keyPair = await Task.Run(() => RsaTool.GenerateKeyPair());
                var symKey = AesTool.GenerateKey();

                var challengeBytes = KeyAgreementPacker.PackChallenge(keyPair.publicKey, symKey);
                var privateKeyBytes = KeyAgreementPacker.PackPrivateKey(keyPair.privateKey);

                StorageManager.WriteKeyAgreementChallenge(challengeBytes, true);
                StorageManager.WriteKeyAgreementPrivateKey(privateKeyBytes);

                DialogShower.ShowInfo("Wygenerowano challenge, przekaż plik dzik-data/challenge-Share drugiej osobie");

                return;
            }

            // if has challenge, and no private keys, generate response, notify user and return;
            if (challenge != null && privateKey == null)
            {
                var (publicKey, exchangeSymKey) = KeyAgreementPacker.UnpackChallenge(challenge);

                var newKeys = MasterKeysGenerator.GenerateMasterKeys();

                var masterKeysEncryptedWithPublicKey = RsaTool.Encrypt(publicKey, newKeys);
                var response = AesTool.Encrypt(exchangeSymKey, masterKeysEncryptedWithPublicKey);

                var readyResponse = Constants.MARKER_KEY_EXCHANGE_RESPONSE_TO_INTERPRETE + Base64PL.StringFromBytes(response);

                StorageManager.WriteMasterKeys(newKeys);
                StorageManager.WriteKeyAgreementResponse(readyResponse);

                var keysVault = MasterKeysPacker.UnpackKeys(newKeys);

                onNewKeysGenerated(keysVault);
                onKeyExchangeResponseReady();
                return;
            }

            // if has challenge and private keys, do nothing here, will handle the response when it arrives, in a separate method.
        }

        internal static void AcceptResponse(string responseWithMarker, Action<KeysVault> onKeysReceivedInExchange)
        {
            // if doesnt have private keys, throw exception
            var privateKeyBytes = StorageManager.ReadKeyAgreementPrivateKeyOrNull();
            if (privateKeyBytes == null) throw new Exception("No private key found");
            var challengeBytes = StorageManager.ReadKeyAgreementChallengeOrNull();
            if (challengeBytes == null) throw new Exception("Could not find challenge file.");

            var privateKey = KeyAgreementPacker.UnpackPrivateKey(privateKeyBytes);
            var (publicKey, exchangeSymKey) = KeyAgreementPacker.UnpackChallenge(challengeBytes);

            // decrypt response and save master secrets.
            var response = responseWithMarker.Substring(Constants.MARKER_KEY_EXCHANGE_RESPONSE_TO_INTERPRETE.Length);
            var responseBytes = Base64PL.BytesFromString(response);

            var decryptedInnedCiphertext = AesTool.Decrypt(exchangeSymKey, responseBytes);
            var plaintextMasterKeys = RsaTool.Decrypt(privateKey, decryptedInnedCiphertext);

            StorageManager.WriteMasterKeys(plaintextMasterKeys);

            var keysVault = MasterKeysPacker.UnpackKeys(plaintextMasterKeys);
            onKeysReceivedInExchange(keysVault);
        }

    }
}
