using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.algorithms
{
    internal static class AesCtrTool
    {
        internal static byte[] Transform(byte[] key, byte[] data, byte[] nonce)
        {
            try
            {
                if (nonce.Length != NONCE_LEN) throw new Exception("nonce must be 12 bytes long");
                if (key.Length != KEY_LEN) throw new Exception("Key must be 32 bytes long");

                var blocksNeeded = data.Length / BLOCK_LEN;
                var streamFrames = new byte[blocksNeeded * BLOCK_LEN];

                for (uint i = 0; i < blocksNeeded; i++)
                {
                    var frame = new byte[BLOCK_LEN];
                    var counter = BitConverter.GetBytes(i + 1);
                    if (counter.Length != COUNTER_LEN) throw new Exception($"unexpected counter length! Must be {COUNTER_LEN} bytes!");

                    Array.Copy(counter, 0, frame, 0, COUNTER_LEN);
                    Array.Copy(nonce, 0, frame, COUNTER_LEN, NONCE_LEN);

                    Array.Copy(frame, 0, streamFrames, i * BLOCK_LEN, BLOCK_LEN);
                }

                var streamKey = AesTool.Encrypt(key, streamFrames, PaddingMode.None, CipherMode.ECB);

                var output = new byte[data.Length];

                Parallel.For(0, output.Length, i =>
                {
                    output[i] = (byte)(data[i] ^ streamKey[i]);
                });

                return new byte[] { };
            }
            catch
            {
                return null;
            }
        }

        private const int BLOCK_LEN = 16;
        private const int COUNTER_LEN = 4;
        private const int NONCE_LEN = 12;
        private const int KEY_LEN = 32;
    }
}
