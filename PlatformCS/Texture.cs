using System;
using System.Runtime.CompilerServices;
using DigBuildPlatformCS.Util;

namespace DigBuildPlatformCS
{
    public sealed class Texture
    {
        internal readonly NativeHandle Handle;

        internal Texture(NativeHandle handle)
        {
            Handle = handle;
        }

        public void Dispose() => Handle.Dispose();
    }

    public readonly ref struct TextureContext
    {
        private readonly NativeHandle _handle;

        internal TextureContext(NativeHandle handle)
        {
            _handle = handle;
        }

        public uint Width => throw new NotImplementedException();
        public uint Height => throw new NotImplementedException();
        public TextureFormat Format => throw new NotImplementedException();
    }

    public enum TextureFormat
    {
    }

    public static class TextureRenderContextExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TextureContext Get(this RenderContext context, Texture texture)
        {
            if (!context.Valid) throw new InvalidRenderContextException();
            return new TextureContext(texture.Handle);
        }
    }
}