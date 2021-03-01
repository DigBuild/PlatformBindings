using System;
using AdvancedDLSupport;

namespace DigBuild.Platform.Util
{
    [NativeSymbols("dbp_native_handle_", SymbolTransformationMethod.Underscore)]
    internal interface INativeHandleBindings
    {
        void Destroy(IntPtr instance);
    }

    internal sealed class NativeHandle : IDisposable
    {
        private static readonly INativeHandleBindings Bindings = NativeLib.Get<INativeHandleBindings>();

        public static readonly NativeHandle Empty = new(IntPtr.Zero, true);

        private readonly IntPtr _ptr;
        private bool _invalid;

        internal NativeHandle(IntPtr ptr, bool allowInvalid = false)
        {
            if (!allowInvalid && ptr == IntPtr.Zero)
                throw new InvalidHandleException();
            _ptr = ptr;
        }

        ~NativeHandle()
        {
            Bindings.Destroy(_ptr);
        }

        public void Dispose()
        {
            if (_invalid)
                throw new InvalidHandleException();
            Bindings.Destroy(_ptr);
            _invalid = true;
            GC.SuppressFinalize(this);
        }

        public static implicit operator IntPtr(NativeHandle handle)
        {
            if (handle._invalid)
                throw new InvalidHandleException();
            return handle._ptr;
        }
    }
}