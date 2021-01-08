using System;
using System.Runtime.CompilerServices;
using DigBuildPlatformCS.Util;

namespace DigBuildPlatformCS
{
    public sealed class Framebuffer
    {
        internal readonly NativeHandle Handle;

        internal Framebuffer(NativeHandle handle)
        {
            Handle = handle;
        }

        public void Dispose() => Handle.Dispose();
    }

    public readonly ref struct FramebufferContext
    {
        private readonly NativeHandle _handle;

        internal FramebufferContext(NativeHandle handle)
        {
            _handle = handle;
        }

        public uint Width => throw new NotImplementedException();
        public uint Height => throw new NotImplementedException();

        public FramebufferAttachment[] Attachments => throw new NotImplementedException();
        public Texture GetTexture(FramebufferAttachment attachment) => throw new NotImplementedException();

        public void SetDrawCommands(params object[] commands) => throw new NotImplementedException();

        public void Update() => throw new NotImplementedException();
    }

    public readonly struct FramebufferAttachment
    {
    }

    public readonly ref struct FramebufferBuilder
    {
        public FramebufferBuilder WithColorAttachment(
            TextureFormat format, out FramebufferAttachment attachment
        ) => throw new NotImplementedException();

        public static implicit operator Framebuffer(FramebufferBuilder builder) => throw new NotImplementedException();
    }

    public static class FramebufferRenderContextExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FramebufferContext Get(this RenderContext context, Framebuffer framebuffer)
        {
            if (!context.Valid) throw new InvalidRenderContextException();
            return new FramebufferContext(framebuffer.Handle);
        }
    }
}