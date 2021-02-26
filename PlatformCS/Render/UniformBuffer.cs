using AdvancedDLSupport;
using DigBuildPlatformCS.Util;
using System;
using System.Runtime.InteropServices;

namespace DigBuildPlatformCS.Render
{
    [NativeSymbols("dbp_uniform_buffer_", SymbolTransformationMethod.Underscore)]
    internal interface IUniformBufferBindings
    {
        void Write(IntPtr instance, IntPtr data, uint dataLength);
    }

    internal static class UniformBuffer
    {
        internal static readonly IUniformBufferBindings Bindings = NativeLib.Get<IUniformBufferBindings>();
    }

    internal interface IUniformBuffer { }

    public class UniformBuffer<T> : IUniformBuffer where T : unmanaged, IUniform<T>
    {
        internal readonly NativeHandle Handle;

        internal UniformBuffer(NativeHandle handle)
        {
            Handle = handle;
        }

        public void Write(NativeBuffer<T> buffer)
        {
            UniformBuffer.Bindings.Write(
                Handle,
                buffer.Ptr,
                (uint) (buffer.Count * Marshal.SizeOf<T>())
            );
        }

        public void Write(PooledNativeBuffer<T> buffer) => Write(buffer.Unpooled);
    }

    public readonly ref struct UniformBufferBuilder<T> where T : unmanaged, IUniform<T>
    {
        private readonly RenderContext _ctx;
        private readonly UniformHandle<T> _uniform;
        private readonly NativeBuffer<T>? _initialData;

        internal UniformBufferBuilder(RenderContext ctx, UniformHandle<T> uniform, NativeBuffer<T>? initialData)
        {
            _ctx = ctx;
            _uniform = uniform;
            _initialData = initialData;
        }

        public static implicit operator UniformBuffer<T>(UniformBufferBuilder<T> builder)
        {
            return new(
                new NativeHandle(
                    RenderContext.Bindings.CreateUniformBuffer(
                        builder._ctx.Ptr,
                        builder._uniform.Shader.Handle,
                        builder._uniform.Binding,
                        builder._initialData?.Ptr ?? IntPtr.Zero,
                        (uint)((builder._initialData?.Count ?? 0) * Marshal.SizeOf<T>())
                    )
                )
            );
        }
    }
}