using Dzik.crypto.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.protocols
{
    internal static class FileEncryptionTool
    {
        private const int maxAllowedExtensionLengthWithDotIncluded = 31;

        internal static FileCryptoOperationResult HandleFile(string path, KeysVault keysVault)
        {
            var extension = Path.GetExtension(path);
            // fix for filenames with dots.
            if (path.LastIndexOf('_') > path.LastIndexOf('.')) extension = string.Empty;

            if (extension == string.Empty)
            {
                return DecryptFile(path, keysVault);
            }
            else
            {
                return EncryptFile(path, keysVault);
            }
        }

        private static FileCryptoOperationResult EncryptFile(string path, KeysVault keysVault)
        {
            try
            {
                var fileBytes = File.ReadAllBytes(path);
                if (fileBytes.Length == 0) return FileCryptoOperationResult.fileIsEmpty;

                // single-out filename and extension related data
                var fileName = Path.GetFileName(path);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                var noDotsFileNameWithoutExtension = fileNameWithoutExtension.Replace('.', '_');
                var extension = Path.GetExtension(path);

                var outputPath = path.WithReplacedLast(fileName, noDotsFileNameWithoutExtension);

                return EncryptDataToFile(outputPath, extension, keysVault, fileBytes);
            }
            catch (Exception)
            {
                return FileCryptoOperationResult.unknownError;
            }
        }

        private static FileCryptoOperationResult DecryptFile(string path, KeysVault keysVault)
        {
            try
            {
                var fileBytes = File.ReadAllBytes(path);
                if (fileBytes.Length == 0) return FileCryptoOperationResult.fileIsEmpty;

                var decryptedBytesWithExtensionData = keysVault.VerifyAndDecrypt(fileBytes);
                if (decryptedBytesWithExtensionData == null) return FileCryptoOperationResult.decryptionError;

                var shouldShowReplayAttackWarning = decryptedBytesWithExtensionData.daysSinceEncryption > Constants.ReplayAttackMaxDaysWithoutWarning;

                var (extension, decryptedFileBytes) = decryptedBytesWithExtensionData.plainText.StripPrefix();

                var outputPath = path + extension;
                File.WriteAllBytes(outputPath, decryptedFileBytes);

                return shouldShowReplayAttackWarning ? FileCryptoOperationResult.decryptedOldCiphertext : FileCryptoOperationResult.decrypted;
            }
            catch (Exception)
            {
                return FileCryptoOperationResult.unknownError;
            }
        }

        private static string WithReplacedLast(this string source, string stringToReplace, string replacementString)
        {
            int place = source.LastIndexOf(stringToReplace);
            if (place == -1) return source;

            string result = source.Remove(place, stringToReplace.Length).Insert(place, replacementString);
            return result;
        }

        private static FileCryptoOperationResult EncryptDataToFile(string outputPath, string extension, KeysVault keysVault, byte[] data)
        {
            try
            {
                // pack extension data into the byte array to be encrypted
                var fileBytesPrefixedWithExtension = data.PrependedWith(extension, maxAllowedExtensionLengthWithDotIncluded);

                var signedEncryptedBytes = keysVault.EncryptAndSign(fileBytesPrefixedWithExtension);
                if (signedEncryptedBytes == null) return FileCryptoOperationResult.unknownError;

                File.WriteAllBytes(outputPath, signedEncryptedBytes);

                return FileCryptoOperationResult.encrypted;
            }
            catch (Exception)
            {
                return FileCryptoOperationResult.unknownError;
            }
        }
    }

    enum FileCryptoOperationResult
    {
        encrypted,
        decrypted,
        decryptedOldCiphertext,
        fileIsEmpty,
        decryptionError,
        unknownError   
    }
}
