using AdvancedDLSupport;
using DigBuildPlatformCS.Util;
using System;

namespace DigBuildPlatformCS.Render
{
    [NativeSymbols("dbp_texture_", SymbolTransformationMethod.Underscore)]
    internal interface ITextureBindings
    {
        uint GetWidth(IntPtr instance);
        uint GetHeight(IntPtr instance);
    }

    public sealed class Texture
    {
        internal static readonly ITextureBindings Bindings = NativeLib.Get<ITextureBindings>();

        internal readonly NativeHandle Handle;

        internal Texture(NativeHandle handle)
        {
            Handle = handle;
        }

        public void Dispose() => Handle.Dispose();

        public uint Width => Bindings.GetWidth(Handle);
        public uint Height => Bindings.GetHeight(Handle);
    }

    public sealed class TextureFormat
    {
        public static readonly TextureFormat R8G8B8A8SRGB = new(0);
        public static readonly TextureFormat B8G8R8A8SRGB = new(1);

        internal readonly byte Id;

        private TextureFormat(byte id)
        {
            Id = id;
        }
    }
}