using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik
{
    internal class Constants
    {
        internal const string MARKER_IMAGE_TAG = "&IMG";
        internal const string MARKER_TO_ENCRYPT_TAG = "$";
        internal const string MARKER_TO_DECRYPT_TAG = "ĘĘĘ";         
        internal const string MARKER_KEY_EXCHANGE_RESPONSE_TO_INTERPRETE = "ęĘę";        
        internal const string MARKER_INSERT_KEY_EXCHANGE_RESPONSE_HERE = "&INSERT_KEY_AGREEMENT_RESPONSE_HERE";        


        public const int MIN_MSG_LENGTH_FOR_COMPRESSION = 212;

        public const int MasterSymmetricKeyLenBytes = 32;
        public const int MasterAuthhKeyLenBytes = 64;

        public const int AesKeysLenBytes = 32;
        public const int RSA_KEY_LENGTH_BITS = 16384;
        /// <summary>
        /// must be set to 12.
        /// </summary>
        public const int GcmNonceLengthBytes = 12;

        /// <summary>
        /// Must be set to 16.
        /// </summary>
        public const int GcmTagLengthBytes = 16;

        /// <summary>
        /// After more than this many days since encryption of the message encountered, 
        /// the replay attack warning will be shown on decryption.
        /// </summary>
        public const int ReplayAttackMaxDaysWithoutWarning = 21;

        /// <summary>
        /// Governs how long it's acceptable to keep most sensitive data in RAM, say master keys.
        /// </summary>
        public const int MostSensitiveDataRamExposureMaxTimeMillis = 5_000;

        /// <summary>
        /// Defines desired difficulty of breaking the password.
        /// </summary>
        public const int KeyDerivationIterationsCount = 1_000_002;
    }
}
