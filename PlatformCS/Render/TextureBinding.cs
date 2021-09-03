using System;
using AdvancedDLSupport;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Render
{
    [NativeSymbols("dbp_texture_binding_", SymbolTransformationMethod.Underscore)]
    internal interface ITextureBindingBindings
    {
        void Update(IntPtr instance, IntPtr sampler, IntPtr texture);
    }

    /// <summary>
    /// A texture binding.
    /// </summary>
    public sealed class TextureBinding
    {
        internal static readonly ITextureBindingBindings Bindings = NativeLib.Get<ITextureBindingBindings>();

        internal readonly NativeHandle Handle;
        internal readonly ShaderSamplerHandle SamplerHandle;

        internal TextureBinding(NativeHandle handle, ShaderSamplerHandle samplerHandle)
        {
            Handle = handle;
            SamplerHandle = samplerHandle;
        }

        /// <summary>
        /// Updates the bound sampler and texture.
        /// </summary>
        /// <param name="sampler">The sampler</param>
        /// <param name="texture">The texture</param>
        public void Update(TextureSampler sampler, Texture texture)
        {
            Bindings.Update(Handle, sampler.Handle, texture.Handle);
        }
    }

    /// <summary>
    /// A texture binding builder.
    /// </summary>
    public readonly ref struct TextureBindingBuilder
    {
        private readonly RenderContext _context;
        private readonly ShaderSamplerHandle _shaderSampler;
        private readonly TextureSampler? _sampler;
        private readonly Texture? _texture;

        internal TextureBindingBuilder(
            RenderContext context,
            ShaderSamplerHandle shaderSampler,
            TextureSampler? sampler,
            Texture? texture
        )
        {
            _context = context;
            _shaderSampler = shaderSampler;
            _sampler = sampler;
            _texture = texture;
        }

        public static implicit operator TextureBinding(TextureBindingBuilder builder)
        {
            return new(
                new NativeHandle(
                    RenderContext.Bindings.CreateTextureBinding(
                        builder._context.Ptr,
                        builder._shaderSampler.Shader.Handle,
                        builder._shaderSampler.Binding,
                        builder._sampler?.Handle ?? IntPtr.Zero, 
                        builder._texture?.Handle ?? IntPtr.Zero
                    )
                ),
                builder._shaderSampler
            );
        }
    }
}