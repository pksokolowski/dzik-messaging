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
        private readonly HashSet<char> initialChars = new HashSet<char>();
        public CompressorPL(string[] vocabulary)
        {
            // populate dictionary
            for (int i = 0; i < vocabulary.Length; i++)
            {
                IntToWord.Add((ushort)i, vocabulary[i]);
                WordToInt.Add(vocabulary[i], (ushort)i);

                initialChars.Add(vocabulary[i][0]);
            }
        }

        internal string Compress(string input)
        {
            var s = input.Replace(COMPRESSOR_MARKER, 'x');

            var builder = new StringBuilder();

            for (int i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (initialChars.Contains(c))
                {
                    var wordEndIndex = IndexOfWordEnd(s, i);
                    var wordSubstring = s.Substring(i, wordEndIndex - i);

                    if (WordToInt.ContainsKey(wordSubstring))
                    {
                        // compress word
                        var vocabIndex = WordToInt[wordSubstring];
                        var encodedIndex = (char)vocabIndex;
                        builder.Append(COMPRESSOR_MARKER);
                        builder.Append(encodedIndex);
                    }
                    else
                    {
                        builder.Append(wordSubstring);
                    }

                    i += wordSubstring.Length - 1;
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        internal string Decompress(string input)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == COMPRESSOR_MARKER)
                {
                    var compressedWordIndex = (ushort)input[i + 1];
                    var word = IntToWord[compressedWordIndex];
                    builder.Append(word);
                }
                else
                {
                    builder.Append(input[i]);
                }
            }
            return builder.ToString();
        }

        private int IndexOfWordEnd(string s, int startIndex)
        {
            for (int i = startIndex; i < s.Length; ++i)
            {
                if (NON_WORD_CHARS.Contains(s[i])) return i;
            }
            return s.Length;
        }

        private const char COMPRESSOR_MARKER = (char)215;
        private HashSet<char> NON_WORD_CHARS = new HashSet<char>() { ' ', ',', '.', ';', '"', '\'', '(', ')', '/' };
    }
}
