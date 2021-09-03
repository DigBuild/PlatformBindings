using System;
using AdvancedDLSupport;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Render
{
    [NativeSymbols("dbp_uniform_binding_", SymbolTransformationMethod.Underscore)]
    internal interface IUniformBindingBindings
    {
        void Update(IntPtr instance, IntPtr uniformBuffer);
    }

    internal static class UniformBinding
    {
        internal static readonly IUniformBindingBindings Bindings = NativeLib.Get<IUniformBindingBindings>();
    }

    internal interface IUniformBinding { }

    /// <summary>
    /// A uniform binding.
    /// </summary>
    /// <typeparam name="TUniform">The uniform type</typeparam>
    public sealed class UniformBinding<TUniform> : IUniformBinding where TUniform : unmanaged, IUniform<TUniform>
    {
        internal readonly NativeHandle Handle;
        internal readonly UniformHandle<TUniform> UniformHandle;

        internal UniformBinding(NativeHandle handle, UniformHandle<TUniform> uniformHandle)
        {
            Handle = handle;
            UniformHandle = uniformHandle;
        }

        /// <summary>
        /// Updates the bound buffer.
        /// </summary>
        /// <param name="buffer">The buffer</param>
        public void Update(UniformBuffer<TUniform> buffer)
        {
            UniformBinding.Bindings.Update(Handle, buffer.Handle);
        }
    }

    /// <summary>
    /// A uniform binding builder.
    /// </summary>
    /// <typeparam name="TUniform">The uniform type</typeparam>
    public readonly ref struct UniformBindingBuilder<TUniform> where TUniform : unmanaged, IUniform<TUniform>
    {
        private readonly RenderContext _context;
        private readonly UniformHandle<TUniform> _uniformHandle;
        private readonly UniformBuffer<TUniform>? _uniformBuffer;

        internal UniformBindingBuilder(
            RenderContext context,
            UniformHandle<TUniform> uniformHandle,
            UniformBuffer<TUniform>? uniformBuffer
        )
        {
            _context = context;
            _uniformHandle = uniformHandle;
            _uniformBuffer = uniformBuffer;
        }

        public static implicit operator UniformBinding<TUniform>(UniformBindingBuilder<TUniform> builder)
        {
            return new(
                new NativeHandle(
                    RenderContext.Bindings.CreateUniformBinding(
                        builder._context.Ptr,
                        builder._uniformHandle.Shader.Handle,
                        builder._uniformHandle.Binding,
                        builder._uniformBuffer?.Handle ?? IntPtr.Zero
                    )
                ),
                builder._uniformHandle
            );
        }
    }
}