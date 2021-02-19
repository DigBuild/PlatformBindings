using DigBuildPlatformCS.Util;
using System;
using System.Numerics;

namespace DigBuildPlatformCS
{
    public sealed class Texture
    {
        private readonly NativeHandle _handle;

        internal Texture(NativeHandle handle)
        {
            _handle = handle;
        }

        public void Dispose() => _handle.Dispose();

        public uint Width => throw new NotImplementedException();
        public uint Height => throw new NotImplementedException();
        public TextureFormat Format => throw new NotImplementedException();
    }
    
    public sealed class TextureUploader
    {
        public void Upload() => throw new NotImplementedException();
    }

    public class TextureFormat
    {
        public static readonly TextureFormat<Vector4> RGBA8 = new();
        
        internal TextureFormat()
        {
        }
    }

    public sealed class TextureFormat<T> : TextureFormat where T : unmanaged
    {
        internal TextureFormat()
        {
        }

        internal Vector4 ToVector4(T value) => throw new NotImplementedException();
    }
}