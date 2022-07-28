using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Pulse.Core
{
    public sealed class SafeUnmanagedArray : SafeBuffer
    {
        public SafeUnmanagedArray(int size)
            : base(true)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            SetHandle(Marshal.AllocHGlobal(size));
            Initialize((ulong)size);
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                Marshal.FreeHGlobal(handle);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public int Length => (int)ByteLength;

        public UnmanagedMemoryStream OpenStream(FileAccess access)
        {
            return new(this, 0, (long)ByteLength, access);
        }
    }
}