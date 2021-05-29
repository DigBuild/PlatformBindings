using System;
using AdvancedDLSupport;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Render
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

    public enum TextureFormat : byte
    {
        R8G8B8A8SRGB,
        B8G8R8A8SRGB,
        R32G32B32A32SFloat
    }
}