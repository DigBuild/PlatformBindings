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

    /// <summary>
    /// A texture.
    /// </summary>
    public sealed class Texture
    {
        internal static readonly ITextureBindings Bindings = NativeLib.Get<ITextureBindings>();

        internal readonly NativeHandle Handle;

        internal Texture(NativeHandle handle)
        {
            Handle = handle;
        }

        public void Dispose() => Handle.Dispose();

        /// <summary>
        /// The width.
        /// </summary>
        public uint Width => Bindings.GetWidth(Handle);
        /// <summary>
        /// The height.
        /// </summary>
        public uint Height => Bindings.GetHeight(Handle);
    }

    /// <summary>
    /// A texture format.
    /// </summary>
    public enum TextureFormat : byte
    {
        R8G8B8A8SRGB,
        B8G8R8A8SRGB,
        R32G32B32A32SFloat
    }
}