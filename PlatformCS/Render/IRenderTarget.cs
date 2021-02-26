using DigBuildPlatformCS.Util;

namespace DigBuildPlatformCS.Render
{
    public interface IRenderTarget
    {
        internal NativeHandle Handle { get; }
        FramebufferFormat Format { get; }
        uint Width { get; }
        uint Height { get; }
    }
}