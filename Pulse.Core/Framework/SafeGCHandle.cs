using System;
using System.Runtime.InteropServices;

namespace Pulse.Core
{
    public sealed class SafeGCHandle : SafeHandle
    {
        public SafeGCHandle(object target, GCHandleType type)
            : base(IntPtr.Zero, true)
        {
            SetHandle(GCHandle.ToIntPtr(GCHandle.Alloc(target, type)));
        }

        public GCHandle Handle => GCHandle.FromIntPtr(handle);

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            try
            {
                GCHandle.FromIntPtr(handle).Free();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IntPtr AddrOfPinnedObject()
        {
            return Handle.AddrOfPinnedObject();
        }
    }
}