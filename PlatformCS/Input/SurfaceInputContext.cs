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

    /// <summary>
    /// The input context for a render surface.
    /// </summary>
    public sealed class SurfaceInputContext
    {
        private static readonly ISurfaceInputContextBindings Bindings = NativeLib.Get<ISurfaceInputContextBindings>();
        
        private readonly IntPtr _ptr;

        internal SurfaceInputContext(IntPtr ptr)
        {
            _ptr = ptr;
        }

        /// <summary>
        /// Runs the callback with every keyboard event in order.
        /// </summary>
        /// <param name="del">The callback</param>
        public void ConsumeKeyboardEvents(KeyboardEventConsumerDelegate del)
        {
            Bindings.ConsumeKeyboardEvents(_ptr, del);
        }
        
        /// <summary>
        /// Runs the callback with every mouse event in order.
        /// </summary>
        /// <param name="del">The callback</param>
        public void ConsumeMouseEvents(MouseEventConsumerDelegate del)
        {
            Bindings.ConsumeMouseEvents(_ptr, del);
        }
        
        /// <summary>
        /// Runs the callback with every scroll event in order.
        /// </summary>
        /// <param name="del">The callback</param>
        public void ConsumeScrollEvents(ScrollEventConsumerDelegate del)
        {
            Bindings.ConsumeScrollEvents(_ptr, del);
        }
        
        /// <summary>
        /// Runs the callback with every cursor event in order.
        /// </summary>
        /// <param name="del">The callback</param>
        public void ConsumeCursorEvents(CursorEventConsumerDelegate del)
        {
            Bindings.ConsumeCursorEvents(_ptr, del);
        }

        /// <summary>
        /// The current cursor mode.
        /// </summary>
        public CursorMode CursorMode
        {
            get => Bindings.GetCursorMode(_ptr);
            set => Bindings.SetCursorMode(_ptr, value);
        }

        /// <summary>
        /// Centers the cursor in the middle of the surface.
        /// </summary>
        public void CenterCursor()
        {
            Bindings.CenterCursor(_ptr);
        }
    }

    /// <summary>
    /// A keyboard action.
    /// </summary>
    public enum KeyboardAction : byte
    {
        Release,
        Press,
        Repeat,
        Character
    }

    /// <summary>
    /// A mouse action.
    /// </summary>
    public enum MouseAction : byte
    {
        Release,
        Press,
    }

    /// <summary>
    /// A cursor action.
    /// </summary>
    public enum CursorAction : byte
    {
        Move,
    }

    /// <summary>
    /// A cursor mode.
    /// </summary>
    public enum CursorMode : byte
    {
        Normal,
        Hidden,
        Raw
    }
}