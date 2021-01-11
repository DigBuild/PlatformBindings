using System;
using System.Runtime.CompilerServices;
using DigBuildPlatformCS.Util;

namespace DigBuildPlatformCS
{
    public sealed class VertexBuffer<TVertex> where TVertex : unmanaged
    {
    }

    public readonly struct VertexBufferWriter<TVertex> where TVertex : unmanaged
    {
    }

    public readonly ref struct VertexBufferWriterContext<TVertex> where TVertex : unmanaged
    {
        public void Write(NativeBuffer<TVertex> buffer) => throw new NotImplementedException();
    }

    public readonly ref struct VertexBufferBuilder<TVertex> where TVertex : unmanaged
    {
        public static implicit operator VertexBuffer<TVertex>(VertexBufferBuilder<TVertex> builder)
            => throw new NotImplementedException();
    }

    public static class VertexBufferRenderContextExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VertexBufferWriterContext<TVertex> Get<TVertex>(this RenderContext context, VertexBufferWriter<TVertex> writer)
            where TVertex : unmanaged
        {
            if (!context.Valid) throw new InvalidRenderContextException();
            return default;//new FramebufferContext(framebuffer.Handle);
        }
    }
}