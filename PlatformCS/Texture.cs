using System;
using DigBuildPlatformCS.Util;

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
    
    public enum TextureFormat
    {
    }
}