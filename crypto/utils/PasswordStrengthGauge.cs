using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.utils
{
    internal static class PasswordStrengthGauge
    {
        /// <summary>
        /// Assumes 500555 interations of PBKDF2 on the password.
        /// 
        /// Bit strength is approximated VERY roughly with the below formula:
        /// 1+ log( ((26)^(15 + 8))*500555 ) / log(2)
        /// </summary>
        internal static PasswordStrength Gauge(string candidate)
        {
            if (candidate.Length < 8)
            {
                return PasswordStrength.WEAK;
            }
            if (candidate.Length < 15)
            {
                return PasswordStrength.CASUAL;
            }

            var numOfExtraCharSpacesCovered = NumOfExtraCoveredCharSpaces(candidate);
            var uniqueChars = NumOfUniqueCharacters(candidate);

            var weightedRepetitionProofedLength = ((3 * candidate.Length) + (1 * uniqueChars)) / 4;

            var score = (2 * numOfExtraCharSpacesCovered) + (weightedRepetitionProofedLength - 15);

            if (score < 5)
            {
                return PasswordStrength.CASUAL;
            }
            if (score < 8)
            {
                return PasswordStrength.SEMI_SERIOUS;
            }
            if (score < 36)
            {
                return PasswordStrength.ENTREPRISE;
            }

            return PasswordStrength.MAXED_OUT;
        }

        private static int NumOfLessObviousChars(string candidate)
        {
            var numOfWhiteSpaces = candidate.Count(char.IsWhiteSpace);
            var numOfUpperCaseLetters = candidate.Count(char.IsUpper);
            var numOfLowerCaseLetters = candidate.Count(char.IsLower);
            var numOfSpecialCharacters = candidate.Length - candidate.Count(char.IsLetterOrDigit) - numOfWhiteSpaces;
            var numOfDigits = candidate.Count(char.IsDigit);

            return numOfUpperCaseLetters + numOfDigits + numOfSpecialCharacters;
        }

        private static int NumOfUniqueCharacters(string candidate)
        {
            var uniques = new HashSet<char>();
            foreach (char c in candidate)
            {
                uniques.Add(c);
            }

            return uniques.Count;
        }

        /// <returns> between 0 and 4 (extra spaces covered)</returns>
        private static int NumOfExtraCoveredCharSpaces(string candidate)
        {
            var spacesCovered = 0;

            if (candidate.Count(char.IsWhiteSpace) > 0) spacesCovered++;
            if (candidate.Count(char.IsUpper) > 0) spacesCovered++;
            if (candidate.Count(char.IsLower) > 0) spacesCovered++;
            if (candidate.Length - candidate.Count(char.IsLetterOrDigit) - candidate.Count(char.IsWhiteSpace) > 0) spacesCovered++;
            if (candidate.Count(char.IsDigit) > 0) spacesCovered++;

            return spacesCovered - 1;
        }

        internal enum PasswordStrength
        {
            WEAK,
            CASUAL,
            SEMI_SERIOUS,
            ENTREPRISE, // around 128 bit in some conditions
            MAXED_OUT, // around 256 bits in some conditions     
        }
    }
}
