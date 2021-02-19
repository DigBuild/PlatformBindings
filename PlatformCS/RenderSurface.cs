using AdvancedDLSupport;
using DigBuildPlatformCS.Util;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace DigBuildPlatformCS
{
    [NativeSymbols("dbp_render_surface_", SymbolTransformationMethod.Underscore)]
    internal interface IRenderSurfaceBindings
    {
        public delegate void GetTitleDelegate(string title);

        uint GetWidth(IntPtr handle);
        uint GetHeight(IntPtr handle);
        void GetTitle(IntPtr handle, GetTitleDelegate del);
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

        internal RenderSurface(NativeHandle handle)
        {
            Handle = handle;
            Closed = Task.Run(() => Bindings.WaitClosed(handle));
        }

        public void Dispose() => Handle.Dispose();

        public Task Close()
        {
            Bindings.Close(Handle);
            return Closed;
        }

        public Task Closed { get; }
    }

    public readonly struct RenderSurfaceContext : IRenderTarget
    {
        private static readonly FramebufferAttachment AttachmentS = new(0, new Vector4(0, 0, 0, 1));
        private static readonly FramebufferFormat FormatS = new(NativeHandle.Empty, 1, new[] { AttachmentS });
        private static readonly RenderStage StageS = new(0, FormatS);

        private readonly NativeHandle _handle;

        internal RenderSurfaceContext(NativeHandle handle)
        {
            _handle = handle;
        }

        NativeHandle IRenderTarget.Handle => _handle;

        public FramebufferFormat Format => FormatS;
        public FramebufferAttachment ColorAttachment => AttachmentS;
        public RenderStage RenderStage => StageS;

        public uint Width
        {
            get => RenderSurface.Bindings.GetWidth(_handle);
            set => RenderSurface.Bindings.SetWidth(_handle, value);
        }

        public uint Height
        {
            get => RenderSurface.Bindings.GetHeight(_handle);
            set => RenderSurface.Bindings.SetHeight(_handle, value);
        }

        public string Title
        {
            get
            {
                string title = null!;
                RenderSurface.Bindings.GetTitle(_handle, s => title = s);
                return title;
            }
            set => RenderSurface.Bindings.SetTitle(_handle, value);
        }

        public bool Fullscreen
        {
            get => RenderSurface.Bindings.IsFullscreen(_handle);
            set => RenderSurface.Bindings.SetFullscreen(_handle, value);
        }

        public bool Visible => RenderSurface.Bindings.IsVisible(_handle);
    }
}