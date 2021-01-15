using System;
using System.Threading.Tasks;
using AdvancedDLSupport;
using DigBuildPlatformCS.Util;

namespace DigBuildPlatformCS
{
    [NativeSymbols("dbp_render_surface_", SymbolTransformationMethod.Underscore)]
    internal interface IRenderSurfaceBindings
    {
        uint GetWidth(IntPtr handle);
        uint GetHeight(IntPtr handle);
        bool IsFullscreen(IntPtr handle);
        bool IsVisible(IntPtr handle);

        void SetWidth(IntPtr handle, uint width);
        void SetHeight(IntPtr handle, uint height);
        void SetTitle(IntPtr handle, string title);
        void SetFullscreen(IntPtr handle, bool fullscreen);

        void Close(IntPtr handle);
        void WaitClosed(IntPtr handle);
    }

    public sealed class RenderSurface : IDisposable
    {
        internal static readonly IRenderSurfaceBindings Bindings = NativeLib.Get<IRenderSurfaceBindings>();

        public delegate void UpdateDelegate(RenderSurfaceContext surface, RenderContext context);

        internal readonly NativeHandle Handle;
        internal string Title;

        internal RenderSurface(NativeHandle handle, string title)
        {
            Handle = handle;
            Title = title;
            Closed = Task.Run(() => Bindings.WaitClosed(handle));
        }

        public void Dispose() => Handle.Dispose();
        
        internal RenderSurfaceContext Context => new(this);

        public Task Close()
        {
            Bindings.Close(Handle);
            return Closed;
        }

        public Task Closed { get; }
    }

    public readonly ref struct RenderSurfaceContext
    {
        private readonly RenderSurface _surface;

        internal RenderSurfaceContext(RenderSurface surface)
        {
            _surface = surface;
        }

        public uint Width
        {
            get => RenderSurface.Bindings.GetWidth(_surface.Handle);
            set => RenderSurface.Bindings.SetWidth(_surface.Handle, value);
        }
        public uint Height
        {
            get => RenderSurface.Bindings.GetHeight(_surface.Handle);
            set => RenderSurface.Bindings.SetHeight(_surface.Handle, value);
        }
        public string Title
        {
            get => _surface.Title;
            set
            {
                RenderSurface.Bindings.SetTitle(_surface.Handle, value);
                _surface.Title = value;
            }
        }
        public bool Fullscreen
        {
            get => RenderSurface.Bindings.IsFullscreen(_surface.Handle);
            set => RenderSurface.Bindings.SetFullscreen(_surface.Handle, value);
        }
        public bool Visible => RenderSurface.Bindings.IsVisible(_surface.Handle);

        public Framebuffer Framebuffer => throw new NotImplementedException();
        public FramebufferAttachment ColorAttachment => throw new NotImplementedException();
        public FramebufferAttachment DepthStencilAttachment => throw new NotImplementedException();
        public RenderStage RenderStage => throw new NotImplementedException();
    }
}