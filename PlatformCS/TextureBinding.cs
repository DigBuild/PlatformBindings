using AdvancedDLSupport;
using DigBuildPlatformCS.Util;
using System;

namespace DigBuildPlatformCS
{
    [NativeSymbols("dbp_texture_binding_", SymbolTransformationMethod.Underscore)]
    internal interface ITextureBindingBindings
    {
        void Update(IntPtr instance, IntPtr sampler, IntPtr texture);
    }

    public sealed class TextureBinding
    {
        internal static readonly ITextureBindingBindings Bindings = NativeLib.Get<ITextureBindingBindings>();

        internal readonly NativeHandle Handle;

        internal TextureBinding(NativeHandle handle)
        {
            Handle = handle;
        }

        public void Update(TextureSampler sampler, Texture texture)
        {
            Bindings.Update(Handle, sampler.Handle, texture.Handle);
        }
    }

    public readonly ref struct TextureBindingBuilder
    {
        private readonly RenderContext _context;
        private readonly ShaderSamplerHandle _shaderSampler;
        private readonly TextureSampler _sampler;
        private readonly Texture _texture;

        internal TextureBindingBuilder(
            RenderContext context,
            ShaderSamplerHandle shaderSampler,
            TextureSampler sampler,
            Texture texture
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
                        builder._sampler.Handle,
                        builder._texture.Handle
                    )
                )
            );
        }
    }
}