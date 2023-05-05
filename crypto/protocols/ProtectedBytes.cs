using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Dzik.crypto.protocols
{
    internal class ProtectedBytes : IDisposable
    {
        private readonly SecureString secure = new SecureString();

        internal ProtectedBytes(PinnedBytes arr)
        {
            if (arr.bytes.Length % 2 != 0)
            {
                throw new Exception("ProtectedBytes only accepts even-len arrays!");
            }
            PutArrayInSecureString(arr.bytes);
        }

        internal PinnedBytes obtainArray()
        {
            return getArrayFromSecureString(secure);
        }

        private void PutArrayInSecureString(byte[] arr)
        {
            for (int i = 0; i < arr.Length; i += 2)
            {
                secure.AppendChar((char)BitConverter.ToInt16(arr, i));
            }
            secure.MakeReadOnly();
        }

        public static PinnedBytes getArrayFromSecureString(SecureString src)
        {
            IntPtr bstr = IntPtr.Zero;
            PinnedBytes pinnedBytes = null;

            try
            {
                // allocate plaintext, unmanaged string bytes
                bstr = Marshal.SecureStringToBSTR(src);
                if (bstr == IntPtr.Zero) return null;

                // copy plaintext bytes to a pinned array
                unsafe
                {
                    byte* bstrBytes = (byte*)bstr;
                    pinnedBytes = new PinnedBytes(src.Length * 2);
                    for (int i = 0; i < pinnedBytes.bytes.Length; i++)
                        pinnedBytes.bytes[i] = *bstrBytes++;
                }

                // clear unmanaged resources
                Marshal.ZeroFreeBSTR(bstr);

                return pinnedBytes;
            }
            catch (Exception)
            {
                if (bstr != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr);
                if (pinnedBytes != null) pinnedBytes.Dispose();

                return null;
            }
        }

        public void Dispose()
        {
            secure.Dispose();
        }
    }
}
