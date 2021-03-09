using System;
using AdvancedDLSupport;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Input
{
    public delegate void KeyboardEventConsumerDelegate(uint scancode, KeyboardAction action);
    public delegate void MouseEventConsumerDelegate(uint button, MouseAction action);
    public delegate void CursorEventConsumerDelegate(uint x, uint y, CursorAction action);

    [NativeSymbols("dbp_surface_input_context_", SymbolTransformationMethod.Underscore)]
    internal interface ISurfaceInputContextBindings
    {
        public void ConsumeKeyboardEvents(IntPtr ptr, KeyboardEventConsumerDelegate del);
        public void ConsumeMouseEvents(IntPtr ptr, MouseEventConsumerDelegate del);
        public void ConsumeCursorEvents(IntPtr ptr, CursorEventConsumerDelegate del);
    }

    public readonly ref struct SurfaceInputContext
    {
        private static readonly ISurfaceInputContextBindings Bindings = NativeLib.Get<ISurfaceInputContextBindings>();
        
        private readonly IntPtr _ptr;

        internal SurfaceInputContext(IntPtr ptr)
        {
            _ptr = ptr;
        }

        public void ConsumeKeyboardEvents(KeyboardEventConsumerDelegate del)
        {
            Bindings.ConsumeKeyboardEvents(_ptr, del);
        }

        public void ConsumeMouseEvents(MouseEventConsumerDelegate del)
        {
            Bindings.ConsumeMouseEvents(_ptr, del);
        }

        public void ConsumeCursorEvents(CursorEventConsumerDelegate del)
        {
            Bindings.ConsumeCursorEvents(_ptr, del);
        }
    }

    public enum KeyboardAction : byte
    {
        Release,
        Press,
        Repeat,
    }

    public enum MouseAction : byte
    {
        Release,
        Press,
    }

    public enum CursorAction : byte
    {
        Move,
    }
}