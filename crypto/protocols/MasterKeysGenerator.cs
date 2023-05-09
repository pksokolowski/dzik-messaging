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

            // Master D
            var masterKeyD = GenerateCompositeRandomKey();

            // Master E
            var masterKeyE = GenerateCompositeRandomKey();

            // Packing
            var keysPacked = MasterKeysPacker.PackKeys(masterKeyA, masterKeyB, masterKeyC, masterKeyD, masterKeyE, masterKeyAuth);

            return keysPacked;
        }

        private static byte[] GenerateCompositeRandomKey()
        {
            var part1 = AesTool.GenerateKey();
            var part2 = new byte[Constants.AesKeysLenBytes];
            RandomNumberGenerator.Create().GetBytes(part2);

            var combined = new byte[Constants.AesKeysLenBytes * 2];
            Array.Copy(part1, 0, combined, 0, Constants.AesKeysLenBytes);
            Array.Copy(part2, 0, combined, Constants.AesKeysLenBytes, Constants.AesKeysLenBytes);

            return Sha256Transform(combined);
        }

        private static byte[] Sha256Transform(byte[] input)
        {
            var sha = SHA256.Create();
            return sha.ComputeHash(input);
        }
    }
}
