using System;

namespace DigBuildPlatformCS.Util
{
    internal sealed class NativeHandle : IDisposable
    {
        private readonly IntPtr _ptr;

        internal NativeHandle(IntPtr ptr)
        {
            _ptr = ptr;
        }

        ~NativeHandle()
        {
            // TODO: Call destroy method
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            // TODO: Call destroy method
        }

        public static implicit operator IntPtr(NativeHandle handle) => handle._ptr;
    }
}