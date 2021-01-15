using System;
using DigBuildPlatformCS.Util;

namespace DigBuildPlatformCS
{
    public sealed class Framebuffer
    {
        private readonly NativeHandle _handle;

        internal Framebuffer(NativeHandle handle)
        {
            _handle = handle;
        }
        public void Dispose() => _handle.Dispose();

        public uint Width => throw new NotImplementedException();
        public uint Height => throw new NotImplementedException();
        
        public Texture Get(FramebufferAttachment attachment) => throw new NotImplementedException();
    }

    public readonly ref struct FramebufferBuilder
    {
        public static implicit operator Framebuffer(FramebufferBuilder builder) => throw new NotImplementedException();
    }
}