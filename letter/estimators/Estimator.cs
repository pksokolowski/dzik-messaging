using Dzik.letter.estimators.utils;
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

        protected abstract double CalculateMultiplier(long elapsedMillis);
        protected abstract double GetStoredMultiplier();
        protected abstract void StoreMultiplier(double multiplier);

        private readonly Stopwatch stopWatch = new Stopwatch();
    }
}
