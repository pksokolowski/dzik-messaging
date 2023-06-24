using Dzik.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.letter.estimators
{
    internal abstract class Estimator
    {
        /// <summary>
        /// Call this when starting operation which is estimated by this estimator
        /// </summary>
        public void Start()
        {
            stopWatch.Start();
        }

        /// <summary>
        /// Call this when the operation to be estimated is over.
        /// </summary>
        public void End()
        {
            stopWatch.Stop();
            var elapsedMillis = stopWatch.ElapsedMilliseconds;

            var multilier = CalculateMultiplier(elapsedMillis);
            StoreMultiplier(multilier);
        }

        /// <returns>
        /// An estimate for the operation about to take place.
        /// </returns>
        public abstract int GetEstimatedMillis();

        protected double GetMedianValue(string listString)
        {
            var multipliers = listString.Split(';');
            if (multipliers.Length == 1)
            {
                return double.Parse(multipliers[0], CultureInfo.InvariantCulture);
            }

            var ordered = multipliers.OrderBy((d) => d).ToList();

            if (ordered.Count % 2 == 0)
            {
                var firstIndex = (ordered.Count / 2) - 1;
                var secondIndex = firstIndex + 1;

                return (Double.Parse(ordered[firstIndex], CultureInfo.InvariantCulture) + Convert.ToDouble(ordered[secondIndex], CultureInfo.InvariantCulture)) / 2d;
            }
            else
            {
                var firstIndex = ordered.Count / 2;
                return Double.Parse(ordered[firstIndex], CultureInfo.InvariantCulture);
            }
        }

        protected string AppendStringList(string listString, double newEntry, int maxListLen)
        {
            var multipliers = listString.Split(';').ToList();
            multipliers.Add(newEntry.ToString());
            if (multipliers.Count > maxListLen) multipliers.RemoveAt(0);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < multipliers.Count; i++)
            {
                sb.Append(Convert.ToDouble(multipliers[i], CultureInfo.InvariantCulture));
                if (i < multipliers.Count - 1) sb.Append(";");
            }
            return sb.ToString();
        }

        protected abstract double CalculateMultiplier(long elapsedMillis);
        protected abstract double GetStoredMultiplier();
        protected abstract void StoreMultiplier(double multiplier);

        private readonly Stopwatch stopWatch = new Stopwatch();
    }

    internal class RtbBytesLoadingEstimator : Estimator
    {
        private readonly long bytesToRead;
        public RtbBytesLoadingEstimator(long bytesToLoad) : base()
        {
            this.bytesToRead = bytesToLoad;
        }

        public override int GetEstimatedMillis()
        {
            var multiplier = GetStoredMultiplier();

            return (int)(bytesToRead * multiplier);
        }

        protected override double CalculateMultiplier(long elapsedMillis)
        {
            return elapsedMillis / (double)bytesToRead;
        }

        protected override double GetStoredMultiplier()
        {
            var listString = Settings.Default.EstimateLoadingXamlMsg;
            var median = GetMedianValue(listString);

            return median;
        }

        protected override void StoreMultiplier(double multiplier)
        {
            var listString = AppendStringList(Settings.Default.EstimateLoadingXamlMsg, multiplier, 5);
            Settings.Default.EstimateLoadingXamlMsg = listString;
        }
    }
}
