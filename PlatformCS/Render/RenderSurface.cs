using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using AdvancedDLSupport;
using DigBuild.Platform.Input;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Render
{
    [NativeSymbols("dbp_render_surface_", SymbolTransformationMethod.Underscore)]
    internal interface IRenderSurfaceBindings
    {
        public delegate void GetTitleDelegate(string title);
        
        IntPtr GetInputContext(IntPtr handle);

        uint GetWidth(IntPtr handle);
        uint GetHeight(IntPtr handle);
        void GetTitle(IntPtr handle, GetTitleDelegate del);
        bool IsFullscreen(IntPtr handle);
        bool IsVisible(IntPtr handle);
        bool IsResized(IntPtr handle);

        void SetWidth(IntPtr handle, uint width);
        void SetHeight(IntPtr handle, uint height);
        void SetTitle(IntPtr handle, string title);
        void SetFullscreen(IntPtr handle, bool fullscreen);

        bool IsActive(IntPtr handle);
        IntPtr UpdateFirst(IntPtr handle);
        void UpdateLast(IntPtr handle);
        void Terminate(IntPtr handle, bool force);
    }

    public sealed class RenderSurface : IDisposable
    {
        internal static readonly IRenderSurfaceBindings Bindings = NativeLib.Get<IRenderSurfaceBindings>();

        public delegate void UpdateDelegate(RenderSurfaceContext surface, RenderContext context);

        private readonly Thread _renderThread;
        private readonly TaskCompletionSource _closedCompletionSource = new();
        private readonly Action _forceStop;

        internal readonly NativeHandle Handle;

        internal RenderSurface(NativeHandle handle, Action forceStop)
        {
            Handle = handle;
            _forceStop = forceStop;
            _renderThread = Thread.CurrentThread;
        }

        public void Dispose()
        {
            _forceStop();
            _renderThread.Join();
            Handle.Dispose();
        }

        public Task Close()
        {
            _forceStop();
            return Closed;
        }

        public Task Closed => _closedCompletionSource.Task;

        internal bool Active => Bindings.IsActive(Handle);

        internal RenderContext UpdateFirst(out bool skip)
        {
            var ctxPtr = Bindings.UpdateFirst(Handle);
            skip = ctxPtr == IntPtr.Zero;
            return new RenderContext(ctxPtr);
        }

        internal void UpdateLast()
        {
            Bindings.UpdateLast(Handle);
        }

        internal void Terminate(bool forced)
        {
            Bindings.Terminate(Handle, forced);
            _closedCompletionSource.SetResult();
        }
    }

    public readonly struct RenderSurfaceContext : IRenderTarget
    {
        private static readonly FramebufferAttachment AttachmentS = new(0, new Vector4(0, 0, 0, 1));
        private static readonly FramebufferFormat FormatS = new(NativeHandle.Empty, new [] { default(RenderStage)! }, new[] { AttachmentS });
        private static readonly RenderStage StageS = new(0, FormatS);

        private readonly NativeHandle _handle;

        internal RenderSurfaceContext(NativeHandle handle)
        {
            _handle = handle;
        }

        NativeHandle IRenderTarget.Handle => _handle;

        public SurfaceInputContext InputContext => new(RenderSurface.Bindings.GetInputContext(_handle));

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
        public bool Resized => RenderSurface.Bindings.IsResized(_handle);
    }
}