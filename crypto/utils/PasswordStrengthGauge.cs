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

            var weightedRepetitionProofedLength = ((2 * candidate.Length) + (1 * uniqueChars)) / 3;

            var score = (2 * numOfExtraCharSpacesCovered) + (weightedRepetitionProofedLength - 15);

            if (score < 5 || uniqueChars < 7)
            {
                return PasswordStrength.CASUAL;
            }
            if (score < 8 || numOfExtraCharSpacesCovered < 3)
            {
                return PasswordStrength.SEMI_SERIOUS;
            }
            if (score < 23)
            {
                return PasswordStrength.ENTREPRISE;
            }
            if (score < 36)
            {
                return PasswordStrength.TOP_SECRET;
            }

            return PasswordStrength.MAXED_OUT;
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
            SEMI_SERIOUS, // around 112 bits in some conditions
            ENTREPRISE,  // around 128 bit in some conditions
            TOP_SECRET, // around 196 bit in some conditions
            MAXED_OUT, // around 256 bits in some conditions     
        }
    }
}
