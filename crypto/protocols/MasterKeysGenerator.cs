using Dzik.common;
using Dzik.crypto.algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.protocols
{
    internal static class MasterKeysGenerator
    {
        internal static byte[] GenerateMasterKeys()
        {
            // Auth key
            var masterKeyAuth = HmacTool.GenerateKey();

            // Master C
            var masterKeyC = new byte[Constants.AesKeysLenBytes];
            RandomNumberGenerator.Create().GetBytes(masterKeyC);

            // Master B
            var masterKeyB = AesTool.GenerateKey();

            // Master A
            var masterKeyA = new byte[Constants.AesKeysLenBytes];
            RandomNumberGenerator.Create().GetBytes(masterKeyA);

            // Packing
            var keysPacked = MasterKeysPacker.PackKeys(masterKeyA, masterKeyB, masterKeyC, masterKeyAuth);

            return keysPacked;
        }
    }
}
