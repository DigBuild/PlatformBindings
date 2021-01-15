using System;
using AdvancedDLSupport;

namespace DigBuildPlatformCS.Util
{
    [NativeSymbols("dbp_native_handle_", SymbolTransformationMethod.Underscore)]
    internal interface INativeHandleBindings
    {
        void Destroy(IntPtr instance);
    }

    internal sealed class NativeHandle : IDisposable
    {
        private static readonly INativeHandleBindings Bindings = NativeLib.Get<INativeHandleBindings>();

        private readonly IntPtr _ptr;

        internal NativeHandle(IntPtr ptr)
        {
            _ptr = ptr;
        }

        ~NativeHandle()
        {
            Bindings.Destroy(_ptr);
        }

        public void Dispose()
        {
            Bindings.Destroy(_ptr);
            GC.SuppressFinalize(this);
        }

        public static implicit operator IntPtr(NativeHandle handle) => handle._ptr;
    }
}