using System;
using AdvancedDLSupport;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Input
{
    public delegate void KeyboardEventConsumerDelegate(uint code, KeyboardAction action);
    public delegate void MouseEventConsumerDelegate(uint button, MouseAction action);
    public delegate void ScrollEventConsumerDelegate(double xOffset, double yOffset);
    public delegate void CursorEventConsumerDelegate(uint x, uint y, CursorAction action);

    [NativeSymbols("dbp_surface_input_context_", SymbolTransformationMethod.Underscore)]
    internal interface ISurfaceInputContextBindings
    {
        public void ConsumeKeyboardEvents(IntPtr ptr, KeyboardEventConsumerDelegate del);
        public void ConsumeMouseEvents(IntPtr ptr, MouseEventConsumerDelegate del);
        public void ConsumeScrollEvents(IntPtr ptr, ScrollEventConsumerDelegate del);
        public void ConsumeCursorEvents(IntPtr ptr, CursorEventConsumerDelegate del);

        CursorMode GetCursorMode(IntPtr ptr);
        void SetCursorMode(IntPtr ptr, CursorMode mode);

        void CenterCursor(IntPtr ptr);
    }

    public sealed class SurfaceInputContext
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

        public void ConsumeScrollEvents(ScrollEventConsumerDelegate del)
        {
            Bindings.ConsumeScrollEvents(_ptr, del);
        }

        public void ConsumeCursorEvents(CursorEventConsumerDelegate del)
        {
            Bindings.ConsumeCursorEvents(_ptr, del);
        }

        public CursorMode CursorMode
        {
            get => Bindings.GetCursorMode(_ptr);
            set => Bindings.SetCursorMode(_ptr, value);
        }

        public void CenterCursor()
        {
            Bindings.CenterCursor(_ptr);
        }
    }

    public enum KeyboardAction : byte
    {
        Release,
        Press,
        Repeat,
        Character
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

    public enum CursorMode : byte
    {
        Normal,
        Hidden,
        Raw
    }
}