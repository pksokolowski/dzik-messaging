using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.algorithms
{
    internal static class DiffusionTool
    {
        internal static byte[] ShifRight(byte[] input, int numOfBytes)
        {
            if (numOfBytes == 0) throw new ArgumentException("shifting by 0 bytes does nothing");
            if (numOfBytes < 0) return ShifLeft(input, -numOfBytes);

            if (numOfBytes >= input.Length) throw new Exception("DIffusion failed because step is too large compared to input length");
            var output = new byte[input.Length];
            Array.Copy(input, 0, output, numOfBytes, input.Length - numOfBytes);
            Array.Copy(input, input.Length - numOfBytes, output, 0, numOfBytes);

            return output;
        }

        private static byte[] ShifLeft(byte[] input, int numOfBytes)
        {
            if (numOfBytes >= input.Length) throw new Exception("DIffusion failed because step is too large compared to input length");
            var output = new byte[input.Length];
            Array.Copy(input, numOfBytes, output, 0, input.Length - numOfBytes);
            Array.Copy(input, 0, output, input.Length - numOfBytes, numOfBytes);

            return output;
        }


    }
}
