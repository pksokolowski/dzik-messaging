using Dzik.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.utils
{
    internal class CompressorPL
    {
        private readonly Dictionary<ushort, string> IntToWord = new Dictionary<ushort, string>();
        private readonly Dictionary<string, ushort> WordToInt = new Dictionary<string, ushort>();

        public CompressorPL(string[] vocabulary)
        {
            // populate dictionary
            for (int i = 0; i < vocabulary.Length; i++)
            {
                if (WordToInt.ContainsKey(vocabulary[i])) continue;
                IntToWord.Add((ushort)i, vocabulary[i]);
                WordToInt.Add(vocabulary[i], (ushort)i);
            }
        }

        internal string Compress(string input)
        {
            var s = input.Replace(NEXT_CHAR_IS_NOT_COMPRESSED_MARKER, 'x');
            var builder = new StringBuilder();

            for (int i = 0; i < s.Length; i++)
            {
                var c = s[i];
                var extraShift = 0;

                if (c >= COMPRESSION_SYMBOLS_ZERO)
                {
                    builder.Append(NEXT_CHAR_IS_NOT_COMPRESSED_MARKER);
                    builder.Append(c);
                    continue;
                }

                for (int ii = PART_MAX_LEN; ii >= PART_MIN_LEN; ii--)
                {
                    if (i + ii >= input.Length) continue;

                    var candidate = s.Substring(i, ii);
                    var candidateCode = CodeForString(candidate);
                    if (candidateCode == -1) continue;

                    c = (char)candidateCode;
                    extraShift = ii - 1;
                    break;
                }

                builder.Append(c);
                i += extraShift;
            }

            var output = builder.ToString();

            // self-test and return compressed version only if decompression yielded the correct input.
            if (Decompress(output) == s) return output; else return s;
        }

        internal string Decompress(string input)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == NEXT_CHAR_IS_NOT_COMPRESSED_MARKER)
                {
                    builder.Append(input[i + 1]);
                    i += 1;
                }
                else
                {
                    if (input[i] >= COMPRESSION_SYMBOLS_ZERO)
                    {
                        var s = CodeToString(input[i]);
                        builder.Append(s);
                    }
                    else
                    {
                        builder.Append(input[i]);
                    }
                }
            }

            return builder.ToString();
        }

        private int CodeForString(string input)
        {
            if (!WordToInt.ContainsKey(input)) return -1;
            return COMPRESSION_SYMBOLS_ZERO + WordToInt[input];
        }

        private string CodeToString(ushort code)
        {
            return IntToWord[(ushort)(code - COMPRESSION_SYMBOLS_ZERO)];
        }

        private const char NEXT_CHAR_IS_NOT_COMPRESSED_MARKER = (char)215;
        private const ushort COMPRESSION_SYMBOLS_ZERO = 1500;

        private const int PART_MAX_LEN = 25;
        private const int PART_MIN_LEN = 3;
    }
}
