using AdvancedDLSupport;
using DigBuildPlatformCS.Util;
using System;

namespace DigBuildPlatformCS
{
    [NativeSymbols("dbp_framebuffer_", SymbolTransformationMethod.Underscore)]
    internal interface IFramebufferBindings
    {
        uint GetWidth(IntPtr handle);
        uint GetHeight(IntPtr handle);
    }

    public sealed class Framebuffer : IDisposable, IRenderTarget
    {
        private static readonly IFramebufferBindings Bindings = NativeLib.Get<IFramebufferBindings>();

        private readonly NativeHandle _handle;

        internal Framebuffer(NativeHandle handle, FramebufferFormat format)
        {
            _handle = handle;
            Format = format;
        }
        public void Dispose() => _handle.Dispose();

        NativeHandle IRenderTarget.Handle => _handle;

        public FramebufferFormat Format { get; }

        public uint Width => Bindings.GetWidth(_handle);
        public uint Height => Bindings.GetHeight(_handle);

        // public Texture Get(FramebufferAttachment attachment) => throw new NotImplementedException();
    }

    public readonly ref struct FramebufferBuilder
    {
        private readonly RenderContext _context;
        private readonly FramebufferFormat _format;
        private readonly uint _width, _height;

        internal FramebufferBuilder(RenderContext context, FramebufferFormat format, uint width, uint height)
        {
            _context = context;
            _format = format;
            _width = width;
            _height = height;
        }

        public static implicit operator Framebuffer(FramebufferBuilder builder)
        {
            return new(
                new NativeHandle(
                    RenderContext.Bindings.CreateFramebuffer(
                        builder._context.Ptr,
                        builder._format.Handle,
                        builder._width,
                        builder._height
                    )
                ),
                builder._format
            );
        }
    }
}