﻿using Dzik.crypto.algorithms;
using Dzik.crypto.utils;
using Dzik.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dzik.crypto.protocols
{
    public sealed class KeysVault : IDisposable
    {
        private readonly ProtectedBytes masterKeyA;
        private readonly ProtectedBytes masterKeyB;
        private readonly ProtectedBytes masterKeyC;
        private readonly ProtectedBytes masterKeyD;
        private readonly ProtectedBytes masterKeyE;
        private readonly ProtectedBytes masterAuthenticationKey;

        private const int envelopeHeaderLenBytes = 112;
        private const int dataKeyLenBytes = 32;
        private const int signatureLenBytes = 32;

        internal KeysVault(PinnedBytes masterA, PinnedBytes masterB, PinnedBytes masterC, PinnedBytes masterD, PinnedBytes masterE, PinnedBytes masterAuthenticationKey)
        {
            this.masterKeyA = new ProtectedBytes(masterA);
            this.masterKeyB = new ProtectedBytes(masterB);
            this.masterKeyC = new ProtectedBytes(masterC);
            this.masterKeyD = new ProtectedBytes(masterD);
            this.masterKeyE = new ProtectedBytes(masterE);
            this.masterAuthenticationKey = new ProtectedBytes(masterAuthenticationKey);

            ((App)Application.Current).Exit += KeysVault_Exit;
        }

        private void KeysVault_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                Dispose();
            }
            catch (Exception) { }
        }

        internal byte[] EncryptAndSign(byte[] data)
        {
            // generate dataKey
            byte[] dataKey = AesTool.GenerateKey();

            // prepend data with unix day stamp
            var timestampedData = UnixDayStamper.PrependWithCurrentStamp(data);
            if (timestampedData == null) return null;

            // encrypt data
            var encryptedData = AesTool.Encrypt(dataKey, timestampedData);
            if (encryptedData == null) return null;

            // generate envelope header
            var header = GenerateEnvelope(dataKey, encryptedData);
            if (header == null) return null;

            // form final message
            var message = new byte[header.Length + encryptedData.Length];
            Array.Copy(header, 0, message, 0, header.Length);
            Array.Copy(encryptedData, 0, message, header.Length, encryptedData.Length);

            return DeterministicCtrHardener.ToggleHardening(masterKeyD, message);
        }

        internal VerifyAndDecryptResult VerifyAndDecrypt(byte[] encryptedHardenedMessage)
        {
            // toggle off hardening
            var encryptedMessage = DeterministicCtrHardener.ToggleHardening(masterKeyD, encryptedHardenedMessage);

            // if entire message is smaller than header size, the format is incorrect.
            if (encryptedMessage.Length < envelopeHeaderLenBytes) return null;

            // separate envelope header and encrypted data
            var envelopeHeader = new byte[envelopeHeaderLenBytes];
            var encryptedData = new byte[encryptedMessage.Length - envelopeHeader.Length];
            Array.Copy(encryptedMessage, 0, envelopeHeader, 0, envelopeHeader.Length);
            Array.Copy(encryptedMessage, envelopeHeader.Length, encryptedData, 0, encryptedData.Length);

            // open the header
            var (signature, dataKey) = OpenEnvelope(envelopeHeader);
            if (signature == null || dataKey == null) return null;

            // verify signature       
            using (var disposableAuthKey = masterAuthenticationKey.obtainArray())
            {
                if (disposableAuthKey == null) return null;
                if (!HmacTool.IsSignatureValidFor(disposableAuthKey.bytes, encryptedData, signature))
                {
                    return null;
                }
            }

            // decrypt data
            var decryptedData = AesTool.Decrypt(dataKey, encryptedData);
            if (decryptedData == null) return null;

            // read approx. decryption time relative to now (in days)
            var daysSinceStampAndData = UnixDayStamper.GetDaysSinceStampAndData(decryptedData);
            if (daysSinceStampAndData == null) return null;

            return new VerifyAndDecryptResult(daysSinceStampAndData.data, daysSinceStampAndData.daysSinceStamp);
        }

        /// <returns>Encrypted envelope' header</returns>
        private byte[] GenerateEnvelope(byte[] dataKey, byte[] encryptedData)
        {
            // signature is generated for the encrypted message
            byte[] signature;
            using (var disposableAuthKey = masterAuthenticationKey.obtainArray())
            {
                if (disposableAuthKey == null) return null;
                signature = HmacTool.ComputeSignature(disposableAuthKey.bytes, encryptedData);
            }
            if (signature == null) return null;

            // envelope data is combined
            byte[] headerData = new byte[signatureLenBytes + dataKeyLenBytes];
            Array.Copy(signature, 0, headerData, 0, signatureLenBytes);
            Array.Copy(dataKey, 0, headerData, signatureLenBytes, dataKeyLenBytes);

            // decrypt 3 keys
            var disposableKeyA = masterKeyA.obtainArray();
            var disposableKeyB = masterKeyB.obtainArray();
            var disposableKeyC = masterKeyC.obtainArray();

            // 3AES-encrypt header data
            var A = AesTool.Encrypt(disposableKeyA.bytes, DiffusionTool.ShifRight(headerData, 8), PaddingMode.None);
            var B = AesTool.Encrypt(disposableKeyB.bytes, DiffusionTool.ShifRight(A, 8), PaddingMode.None);
            var C = AesTool.Encrypt(disposableKeyC.bytes, DiffusionTool.ShifRight(B, 8), PaddingMode.None);

            // dispose of the plaintext keys
            disposableKeyA?.Dispose();
            disposableKeyB?.Dispose();
            disposableKeyC?.Dispose();

            // return encrypted envelope header
            return C;
        }

        /// <returns>Signature and dataKey</returns>
        private (byte[], byte[]) OpenEnvelope(byte[] encryptedEnvelopeHeader)
        {
            if (encryptedEnvelopeHeader.Length != envelopeHeaderLenBytes) return (null, null);

            // decrypt 3 keys
            var disposableKeyA = masterKeyA.obtainArray();
            var disposableKeyB = masterKeyB.obtainArray();
            var disposableKeyC = masterKeyC.obtainArray();
            if (disposableKeyA == null || disposableKeyB == null || disposableKeyC == null)
            {
                // dispose of the plaintext keys
                disposableKeyA?.Dispose();
                disposableKeyB?.Dispose();
                disposableKeyC?.Dispose();
                return (null, null);
            }

            try
            {
                var B = DiffusionTool.ShifRight(AesTool.Decrypt(disposableKeyC.bytes, encryptedEnvelopeHeader, PaddingMode.None), -8);
                var A = DiffusionTool.ShifRight(AesTool.Decrypt(disposableKeyB.bytes, B, PaddingMode.None), -8);
                var envelopeHeader = DiffusionTool.ShifRight(AesTool.Decrypt(disposableKeyA.bytes, A, PaddingMode.None), -8);

                if (envelopeHeader == null) return (null, null);

                var signature = new byte[signatureLenBytes];
                var dataKey = new byte[dataKeyLenBytes];

                Array.Copy(envelopeHeader, 0, signature, 0, signatureLenBytes);
                Array.Copy(envelopeHeader, signatureLenBytes, dataKey, 0, dataKeyLenBytes);

                return (signature, dataKey);
            }
            catch (Exception)
            {
                return (null, null);
            }
            finally
            {
                // dispose of the plaintext keys
                disposableKeyA?.Dispose();
                disposableKeyB?.Dispose();
                disposableKeyC?.Dispose();
            }
        }

        public void Dispose()
        {
            ((App)Application.Current).Exit -= KeysVault_Exit;
            masterKeyA.Dispose();
            masterKeyB.Dispose();
            masterKeyC.Dispose();
            masterAuthenticationKey.Dispose();
        }
    }

    class VerifyAndDecryptResult
    {
        public byte[] plainText;
        public long daysSinceEncryption;

        public VerifyAndDecryptResult(byte[] plainText, long daysSinceEncryption)
        {
            this.plainText = plainText;
            this.daysSinceEncryption = daysSinceEncryption;
        }
    }
}
