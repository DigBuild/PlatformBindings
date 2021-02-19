using DigBuildPlatformCS.Util;

namespace DigBuildPlatformCS
{
    public interface IRenderTarget
    {
        internal NativeHandle Handle { get; }
        FramebufferFormat Format { get; }
        uint Width { get; }
        uint Height { get; }
    }
}