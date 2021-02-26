using DigBuildPlatformCS.Util;

namespace DigBuildPlatformCS.Render
{
    public sealed class TextureSampler
    {
        internal readonly NativeHandle Handle;

        internal TextureSampler(NativeHandle handle)
        {
            Handle = handle;
        }
    }

    public enum TextureFiltering : byte
    {
        Linear,
        Nearest
    }

    public enum TextureWrapping : byte
    {
        Repeat, MirroredRepeat,
        ClampToEdge, MirroredClampToEdge,
        ClampToBorder
    }

    public enum TextureBorderColor : byte
    {
        TransparentBlack,
        OpaqueBlack,
        OpaqueWhite
    }

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