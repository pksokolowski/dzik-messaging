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


        public const int MasterSymmetricKeyLenBytes = 32;
        public const int MasterAuthhKeyLenBytes = 128;

        public const int AesKeysLenBytes = 32;
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

    }
}
