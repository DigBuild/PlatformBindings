using System;
using System.Runtime.InteropServices;
using AdvancedDLSupport;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Render
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
    
    public class UniformBuffer<T> where T : unmanaged, IUniform<T>
    {
        internal readonly NativeHandle Handle;

        internal UniformBuffer(NativeHandle handle)
        {
            Handle = handle;
        }

        public void Write(INativeBuffer<T> buffer)
        {
            UniformBuffer.Bindings.Write(
                Handle,
                buffer.Ptr,
                (uint) (buffer.Count * Marshal.SizeOf<T>())
            );
        }
    }

    public readonly ref struct UniformBufferBuilder<T> where T : unmanaged, IUniform<T>
    {
        private readonly RenderContext _ctx;
        private readonly INativeBuffer<T>? _initialData;

        internal UniformBufferBuilder(RenderContext ctx, INativeBuffer<T>? initialData)
        {
            _ctx = ctx;
            _initialData = initialData;
        }

        public static implicit operator UniformBuffer<T>(UniformBufferBuilder<T> builder)
        {
            return new(
                new NativeHandle(
                    RenderContext.Bindings.CreateUniformBuffer(
                        builder._ctx.Ptr,
                        builder._initialData?.Ptr ?? IntPtr.Zero,
                        (uint)((builder._initialData?.Count ?? 0) * Marshal.SizeOf<T>())
                    )
                )
            );
        }
    }
}