using System;
using DigBuildPlatformCS.Util;

namespace DigBuildPlatformCS
{
    public sealed class VertexBuffer<TVertex> where TVertex : unmanaged
    {
    }

    public readonly struct VertexBufferWriter<TVertex> where TVertex : unmanaged
    {
        public void Write(NativeBuffer<TVertex> buffer) => throw new NotImplementedException();
        public void Write(PooledNativeBuffer<TVertex> buffer) => Write(buffer.Unpooled);
    }

    public readonly ref struct VertexBufferBuilder<TVertex> where TVertex : unmanaged
    {
        public static implicit operator VertexBuffer<TVertex>(VertexBufferBuilder<TVertex> builder)
            => throw new NotImplementedException();
    }
}