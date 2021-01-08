using System;
using System.Threading.Tasks;
using DigBuildPlatformCS.Util;

namespace DigBuildPlatformCS
{
    public sealed class RenderSurface : IDisposable
    {
        public delegate void UpdateDelegate(RenderSurfaceContext surface, RenderContext context);

        private readonly NativeHandle _handle;

        internal RenderSurface(NativeHandle handle)
        {
            _handle = handle;
        }

        public void Dispose() => _handle.Dispose();

        public Task Closed => throw new NotImplementedException();
        public Task Close() => throw new NotImplementedException();
    }

    public readonly ref struct RenderSurfaceContext
    {
        private readonly NativeHandle _handle;

        public uint Width => throw new NotImplementedException();
        public uint Height => throw new NotImplementedException();
        public string Title => throw new NotImplementedException();
        
        public bool Visible => throw new NotImplementedException();
        public bool Fullscreen => throw new NotImplementedException();

        public FramebufferContext Framebuffer { get; }
    }

}