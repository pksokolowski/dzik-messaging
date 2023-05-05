using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.protocols
{
    internal class PinnedBytes : IDisposable
    {
        internal byte[] bytes { get; private set; }
        GCHandle handle;
        private readonly SensitiveDataExposureAlarmist Alarmist = new SensitiveDataExposureAlarmist();

        internal PinnedBytes(int length)
        {
            bytes = new byte[length];
            handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            Alarmist.StartEggTimer();
        }

        internal PinnedBytes(byte[] bytes)
        {
            this.bytes = bytes;
            handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        }

        public void Dispose()
        {
            // zero the bytes out
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = 0;
            }
            handle.Free();
            // make sure it cannot be used anymore.
            bytes = null;

            Alarmist.CancelEggTimer();
        }
    }
}
