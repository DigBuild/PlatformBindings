using DigBuild.Platform.Util;

namespace DigBuild.Platform.Render
{
    /// <summary>
    /// A texture sampler.
    /// </summary>
    public sealed class TextureSampler
    {
        internal readonly NativeHandle Handle;

        internal TextureSampler(NativeHandle handle)
        {
            Handle = handle;
        }
    }

    /// <summary>
    /// A texture filtering method.
    /// </summary>
    public enum TextureFiltering : byte
    {
        Linear,
        Nearest
    }

    /// <summary>
    /// A texture wrapping method.
    /// </summary>
    public enum TextureWrapping : byte
    {
        Repeat, MirroredRepeat,
        ClampToEdge, MirroredClampToEdge,
        ClampToBorder
    }

    /// <summary>
    /// A texture border color.
    /// </summary>
    public enum TextureBorderColor : byte
    {
        TransparentBlack,
        OpaqueBlack,
        OpaqueWhite
    }

    /// <summary>
    /// A texture sampler builder.
    /// </summary>
    public readonly ref struct TextureSamplerBuilder
    {
        private readonly RenderContext _context;
        private readonly TextureFiltering _minFiltering;
        private readonly TextureFiltering _maxFiltering;
        private readonly TextureWrapping _wrapping;
        private readonly TextureBorderColor _borderColor;
        private readonly bool _enableAnisotropy;
        private readonly uint _anisotropyLevel;

        internal TextureSamplerBuilder(
            RenderContext context,
            TextureFiltering minFiltering,
            TextureFiltering maxFiltering,
            TextureWrapping wrapping,
            TextureBorderColor borderColor,
            bool enableAnisotropy = false,
            uint anisotropyLevel = 0
        )
        {
            _context = context;
            _minFiltering = minFiltering;
            _maxFiltering = maxFiltering;
            _wrapping = wrapping;
            _borderColor = borderColor;
            _enableAnisotropy = enableAnisotropy;
            _anisotropyLevel = anisotropyLevel;
        }

        /// <summary>
        /// Enables anisotropic filtering.
        /// </summary>
        /// <param name="level">The level of filtering</param>
        /// <returns>The builder</returns>
        public TextureSamplerBuilder WithAnisotropicFiltering(uint level)
        {
            return new(
                _context, _minFiltering, _maxFiltering,
                _wrapping, _borderColor,
                true, level
            );
        }

        public static implicit operator TextureSampler(TextureSamplerBuilder builder)
        {
            return new(
                new NativeHandle(
                    RenderContext.Bindings.CreateTextureSampler(
                        builder._context.Ptr,
                        builder._minFiltering,
                        builder._maxFiltering,
                        builder._wrapping,
                        builder._borderColor,
                        builder._enableAnisotropy,
                        builder._anisotropyLevel
                    )
                )
            );
        }
    }
}