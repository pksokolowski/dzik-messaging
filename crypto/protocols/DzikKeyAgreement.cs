using Dzik.common;
using Dzik.crypto.algorithms;
using Dzik.crypto.utils;
using Dzik.data;
using Dzik.keyStorageWindows;
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
        internal static void Initialize(Action<KeysVault> onNewKeysGenerated, Action onKeyExchangeResponseReady)
        {
            var challenge = StorageManager.ReadKeyAgreementChallengeOrNull();
            var privateKey = StorageManager.ReadKeyAgreementPrivateKeyOrNull();
            // if has nothing, generate private keys and challenge, then return
            if (challenge == null && privateKey == null)
            {
                new PrepareChallengeWindow().Show();
                return;
            }

            // if has challenge, and no private keys, generate response, notify user and return;
            if (challenge != null && privateKey == null)
            {
                new GenerateResponseWindow(onNewKeysGenerated, onKeyExchangeResponseReady, challenge).Show();
                return;
            }

            // if has challenge and private keys, do nothing here, will handle the response when it arrives, in a separate method.
        }

        internal static void AcceptResponse(string responseWithMarker, Action<KeysVault> onKeysReceivedInExchange)
        {
            new ReceiveResponseWindow(responseWithMarker, onKeysReceivedInExchange).Show();
        }

        /// <returns>random KEK used to encrypt the private key</returns>
        internal static async Task<byte[]> GeneratePrivateKeyAndChallengeAndReturnKEK()
        {
            var keyPair = await Task.Run(() => RsaTool.GenerateKeyPair());
            var symKey = AesTool.GenerateKey();

            var challengeBytes = KeyAgreementPacker.PackChallenge(keyPair.publicKey, symKey);
            var privateKeyBytes = KeyAgreementPacker.PackPrivateKey(keyPair.privateKey);

            var (encryptedPrivKey, kek) = RandomKeyBasedEncryptor.Encrypt(privateKeyBytes);

            StorageManager.WriteKeyAgreementChallenge(challengeBytes, true);
            StorageManager.WriteKeyAgreementPrivateKey(encryptedPrivKey);

            return kek;
        }

        internal static void GenerateResponse(Action<KeysVault> onNewKeysGenerated, Action onKeyExchangeResponseReady, byte[] challenge, string passwordOrNull)
        {
            var (publicKey, exchangeSymKey) = KeyAgreementPacker.UnpackChallenge(challenge);

            var newKeys = MasterKeysGenerator.GenerateMasterKeys();

            var masterKeysEncryptedWithPublicKey = RsaTool.Encrypt(publicKey, newKeys);
            var response = AesTool.Encrypt(exchangeSymKey, masterKeysEncryptedWithPublicKey);

            var readyResponse = Constants.MARKER_KEY_EXCHANGE_RESPONSE_TO_INTERPRETE + Base256.StringFromBytes(response);

            if (passwordOrNull != null)
            {
                // with password:        
                StorageManager.WritePasswordProtectedMasterKeys(newKeys, passwordOrNull);
            }
            else
            {
                // without password:
                StorageManager.WriteMasterKeys(newKeys);
            }

            StorageManager.WriteKeyAgreementResponse(readyResponse);

            var keysVault = MasterKeysPacker.UnpackKeys(newKeys);

            onNewKeysGenerated(keysVault);
            onKeyExchangeResponseReady();
        }

        internal static void AcceptResponse(string responseWithMarker, byte[] privKeyKEK, string passwordOrNull, Action<KeysVault> onKeysReceivedInExchange)
        {
            // if doesnt have private keys, throw exception
            var encryptedPrivateKeyBytes = StorageManager.ReadKeyAgreementPrivateKeyOrNull() ?? throw new Exception("No private key found");
            var challengeBytes = StorageManager.ReadKeyAgreementChallengeOrNull() ?? throw new Exception("Could not find challenge file.");

            var privateKeyBytes = RandomKeyBasedEncryptor.Decrypt(privKeyKEK, encryptedPrivateKeyBytes);

            var privateKey = KeyAgreementPacker.UnpackPrivateKey(privateKeyBytes);
            var (publicKey, exchangeSymKey) = KeyAgreementPacker.UnpackChallenge(challengeBytes);

            // decrypt response and save master secrets.
            var response = responseWithMarker.Substring(Constants.MARKER_KEY_EXCHANGE_RESPONSE_TO_INTERPRETE.Length);
            var responseBytes = Base256.BytesFromString(response);

            var decryptedInnedCiphertext = AesTool.Decrypt(exchangeSymKey, responseBytes);
            var plaintextMasterKeys = RsaTool.Decrypt(privateKey, decryptedInnedCiphertext);

            if (passwordOrNull != null)
            {
                // with password:        
                StorageManager.WritePasswordProtectedMasterKeys(plaintextMasterKeys, passwordOrNull);
            }
            else
            {
                // without password:
                StorageManager.WriteMasterKeys(plaintextMasterKeys);
            }

            var keysVault = MasterKeysPacker.UnpackKeys(plaintextMasterKeys);
            onKeysReceivedInExchange(keysVault);
        }

    }
}
