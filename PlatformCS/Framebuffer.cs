using AdvancedDLSupport;
using DigBuildPlatformCS.Util;
using System;
using System.Collections.Generic;

namespace DigBuildPlatformCS
{
    [NativeSymbols("dbp_framebuffer_", SymbolTransformationMethod.Underscore)]
    internal interface IFramebufferBindings
    {
        uint GetWidth(IntPtr handle);
        uint GetHeight(IntPtr handle);

        IntPtr GetTexture(IntPtr handle, uint attachment);
    }

    public sealed class Framebuffer : IDisposable, IRenderTarget
    {
        internal static readonly IFramebufferBindings Bindings = NativeLib.Get<IFramebufferBindings>();

        private readonly NativeHandle _handle;
        private readonly List<Texture> _textures;

        internal Framebuffer(NativeHandle handle, FramebufferFormat format, List<Texture> textures)
        {
            _handle = handle;
            Format = format;
            _textures = textures;
        }
        public void Dispose() => _handle.Dispose();

        NativeHandle IRenderTarget.Handle => _handle;

        public FramebufferFormat Format { get; }

        public uint Width => Bindings.GetWidth(_handle);
        public uint Height => Bindings.GetHeight(_handle);

        public Texture Get(FramebufferAttachment attachment) => _textures[(int) attachment.Id];
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
            var handle = new NativeHandle(
                RenderContext.Bindings.CreateFramebuffer(
                    builder._context.Ptr,
                    builder._format.Handle,
                    builder._width,
                    builder._height
                )
            );
            var textures = new List<Texture>();
            for (var i = 0u; i < builder._format.Attachments.Count; i++)
                textures.Add(new Texture(new NativeHandle(
                    Framebuffer.Bindings.GetTexture(handle, i)
                )));
            return new Framebuffer(
                handle,
                builder._format,
                textures
            );
        }
    }
}