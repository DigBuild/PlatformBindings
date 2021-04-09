using System;
using System.Runtime.InteropServices;
using AdvancedDLSupport;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Render
{
    [NativeSymbols("dbp_vertex_buffer_", SymbolTransformationMethod.Underscore)]
    internal interface IVertexBufferBindings
    {
        void Write(IntPtr instance, IntPtr data, uint dataLength);
    }

    internal static class VertexBuffer
    {
        internal static readonly IVertexBufferBindings Bindings = NativeLib.Get<IVertexBufferBindings>();
    }

    public sealed class VertexBuffer<TVertex> where TVertex : unmanaged
    {
        internal readonly NativeHandle Handle;

        internal VertexBuffer(NativeHandle handle)
        {
            Handle = handle;
        }
    }

    public sealed class VertexBufferWriter<TVertex> where TVertex : unmanaged
    {
        internal NativeHandle Handle = null!;

        public void Write(INativeBuffer<TVertex> buffer)
        {
            VertexBuffer.Bindings.Write(
                Handle,
                buffer.Ptr,
                buffer.Count
            );
        }
    }

    public readonly ref struct VertexBufferBuilder<TVertex> where TVertex : unmanaged
    {
        private readonly RenderContext _ctx;
        private readonly INativeBuffer<TVertex>? _initialData;
        private readonly VertexBufferWriter<TVertex>? _writer;

        internal VertexBufferBuilder(RenderContext ctx, INativeBuffer<TVertex>? initialData)
        {
            _ctx = ctx;
            _initialData = initialData;
            _writer = null;
        }
        internal VertexBufferBuilder(RenderContext ctx, INativeBuffer<TVertex>? initialData, out VertexBufferWriter<TVertex> writer)
        {
            _ctx = ctx;
            _initialData = initialData;
            _writer = writer = new VertexBufferWriter<TVertex>();
        }

        public static implicit operator VertexBuffer<TVertex>(VertexBufferBuilder<TVertex> builder)
        {
            var handle = new NativeHandle(
                RenderContext.Bindings.CreateVertexBuffer(
                    builder._ctx.Ptr,
                    builder._initialData?.Ptr ?? IntPtr.Zero,
                    builder._initialData?.Count ?? 0,
                    Marshal.SizeOf<TVertex>(),
                    builder._writer != null
                )
            );
            if (builder._writer != null)
                builder._writer.Handle = handle;
            return new VertexBuffer<TVertex>(handle);
        }
    }
}