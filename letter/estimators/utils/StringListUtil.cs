using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.letter.estimators.utils
{
    internal static class StringListUtil
    {
        internal static double GetMedianValue(string listString)
        {
            var multipliers = listString.Split(';').ToList().ConvertAll((s) => double.Parse(s, CultureInfo.InvariantCulture));
            if (multipliers.Count == 1)
            {
                return multipliers[0];
            }

            var ordered = multipliers.OrderBy((d) => d).ToList();

            if (ordered.Count % 2 == 0)
            {
                var firstIndex = (ordered.Count / 2) - 1;
                var secondIndex = firstIndex + 1;

                return (ordered[firstIndex] + ordered[secondIndex]) / 2d;
            }
            else
            {
                var firstIndex = ordered.Count / 2;
                return ordered[firstIndex];
            }
        }

        internal static string AppendStringList(string listString, double newEntry, int maxListLen)
        {
            var multipliers = listString.Split(';').ToList();
            multipliers.Add(newEntry.ToString(CultureInfo.InvariantCulture));
            if (multipliers.Count > maxListLen) multipliers.RemoveAt(0);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < multipliers.Count; i++)
            {
                sb.Append(multipliers[i]);
                if (i < multipliers.Count - 1) sb.Append(";");
            }
            return sb.ToString();
        }
    }
}
