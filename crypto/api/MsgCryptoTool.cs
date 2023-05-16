using Dzik.crypto.algorithms;
using Dzik.crypto.protocols;
using Dzik.crypto.utils;
using Dzik.data;
using Dzik.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.api
{
    internal class MsgCryptoTool : Encryptor, Decryptor
    {
        private KeysVault keysVault;
        private CompressorPL compressorPL = new CompressorPL(PolishVocabularyLoader.GetVocabulary());

        internal MsgCryptoTool(KeysVault keysVault)
        {
            this.keysVault = keysVault;
        }

        public string Encrypt(string plainText)
        {
            var plainTextCompressedInfNeeded = CompressIfNeeded(plainText);
            var cipherTextBytes = keysVault.EncryptAndSign(plainTextCompressedInfNeeded);
            var ciphertextString = Base256.StringFromBytes(cipherTextBytes);

            return ciphertextString;
        }


        public DecryptedMsg Decrypt(string cipherText)
        {
            var cipherTextBytes = Base256.BytesFromString(cipherText);
            var verifyAndDecryptResult = keysVault.VerifyAndDecrypt(cipherTextBytes);
            var plainTextString = HandleCompressionIfAny(verifyAndDecryptResult.plainText);            

            return new DecryptedMsg(plainTextString, verifyAndDecryptResult.daysSinceEncryption);
        }

        private byte[] CompressIfNeeded(string plaintext)
        {
            if (plaintext.Length < Constants.MIN_MSG_LENGTH_FOR_COMPRESSION)
            {
                var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                return DataPrependedWithByte(plaintextBytes, NO_COMPRESSION_MARKER);
            }
            else
            {
                // precompress
                var precompressedPlaintext = compressorPL.Compress(plaintext);

                // compress
                var plaintextBytes = Encoding.UTF8.GetBytes(precompressedPlaintext);
                var compressedPlaintext = GzipCompressor.Compress(plaintextBytes);

                // mark as compressed
                return DataPrependedWithByte(compressedPlaintext, GenerateRandomCompressionMarker());
            }
        }

        private string HandleCompressionIfAny(byte[] plaintext)
        {
            var plaintextWithoutMarkers = StripCompressionRelatedMarkers(plaintext);

            if (plaintext[0] == NO_COMPRESSION_MARKER)
            {
                return Encoding.UTF8.GetString(plaintextWithoutMarkers);
            }
            else
            {
                var decompressedPlaintextBytes = GzipCompressor.Decompress(plaintextWithoutMarkers);
                var plaintextString = Encoding.UTF8.GetString(decompressedPlaintextBytes);

                // undo precompression
                var plaintextDecompressed = compressorPL.Decompress(plaintextString);

                return plaintextDecompressed;
            }
        }

        private byte GenerateRandomCompressionMarker()
        {
            var markerArray = new byte[1];
            new Random().NextBytes(markerArray);
            var marker = markerArray[0];

            if (marker == NO_COMPRESSION_MARKER) marker = NO_COMPRESSION_MARKER + 1;

            return marker;
        }

        private byte[] DataPrependedWithByte(byte[] input, byte marker)
        {
            var output = new byte[input.Length + 1];

            output[0] = marker;
            Array.Copy(input, 0, output, 1, input.Length);

            return output;
        }

        private byte[] StripCompressionRelatedMarkers(byte[] markedArray)
        {
            var output = new byte[markedArray.Length - 1];
            Array.Copy(markedArray, 1, output, 0, output.Length);

            return output;
        }

        private const byte NO_COMPRESSION_MARKER = 5;
    }
}
