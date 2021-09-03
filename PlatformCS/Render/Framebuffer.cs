using System;
using System.Collections.Generic;
using AdvancedDLSupport;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Render
{
    [NativeSymbols("dbp_framebuffer_", SymbolTransformationMethod.Underscore)]
    internal interface IFramebufferBindings
    {
        uint GetWidth(IntPtr handle);
        uint GetHeight(IntPtr handle);

        IntPtr GetTexture(IntPtr handle, uint attachment);
    }

    /// <summary>
    /// A framebuffer.
    /// </summary>
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

        /// <summary>
        /// The format.
        /// </summary>
        public FramebufferFormat Format { get; }

        /// <summary>
        /// The width.
        /// </summary>
        public uint Width => Bindings.GetWidth(_handle);
        /// <summary>
        /// The height.
        /// </summary>
        public uint Height => Bindings.GetHeight(_handle);

        /// <summary>
        /// Gets the texture for the specified attachment.
        /// </summary>
        /// <param name="attachment">The attachment</param>
        /// <returns>The texture</returns>
        public Texture Get(FramebufferAttachment attachment) => _textures[(int) attachment.Id];
    }

    /// <summary>
    /// A framebuffer builder.
    /// </summary>
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