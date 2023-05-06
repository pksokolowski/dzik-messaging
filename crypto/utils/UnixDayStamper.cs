using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.utils
{
    internal static class UnixDayStamper
    {
        internal const int StampLenBytes = 2;

        internal static byte[] PrependWithCurrentStamp(byte[] data)
        {
            var now = GetCurrentStamp();
            return ByteArrayModificationUtils.PrependWithConstantLenPrefix(now, data);
        }

        internal static DaysSinceStampAndData GetDaysSinceStampAndData(byte[] stampedData)
        {
            var prefixAndData = ByteArrayModificationUtils.StripConstantLenPrefix(StampLenBytes, stampedData);
            if (prefixAndData == null) return null;

            var daysSinceStamp = DaysSinceStamp(prefixAndData.prefix);

            return new DaysSinceStampAndData(daysSinceStamp, prefixAndData.data);
        }

        internal static byte[] GetCurrentStamp()
        {
            var now = UnixDaysNow();
            var nowBytes = GetLittleEndianBytes(now, StampLenBytes);

            return nowBytes;
        }

        internal static long DaysSinceStamp(byte[] stampBytes)
        {
            var stamp = GetLongFromLittleEndianBytes(stampBytes);
            var now = UnixDaysNow();

            return now - stamp;
        }

        private static long UnixDaysNow() => new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() / 86400L;

        private static byte[] GetLittleEndianBytes(long unixStampSeconds, int LengthWanted)
        {
            var bytes = BitConverter.GetBytes(unixStampSeconds);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            var output = new byte[LengthWanted];
            Array.Copy(bytes, 0, output, 0, LengthWanted);

            return output;
        }

        private static long GetLongFromLittleEndianBytes(byte[] bytes)
        {
            var bytesCorrected = new byte[8];
            Array.Copy(bytes, 0, bytesCorrected, 0, bytes.Length);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytesCorrected);

            return BitConverter.ToInt64(bytesCorrected, 0);
        }
    }

    internal class DaysSinceStampAndData
    {
        public long daysSinceStamp;
        public byte[] data;
        public DaysSinceStampAndData(long daysSinceStamp, byte[] data)
        {
            this.daysSinceStamp = daysSinceStamp;
            this.data = data;
        }
    }
}
