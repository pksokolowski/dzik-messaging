using Dzik.letter.estimators.utils;
using Dzik.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.letter.estimators
{
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
            var median = StringListUtil.GetMedianValue(listString);

            return median;
        }

        protected override void StoreMultiplier(double multiplier)
        {
            var listString = StringListUtil.AppendStringList(Settings.Default.EstimateLoadingXamlMsg, multiplier, 5);
            Settings.Default.EstimateLoadingXamlMsg = listString;
        }
    }
}
