using DigBuild.Platform.Util;

namespace DigBuild.Platform.Render
{
    /// <summary>
    /// A render target.
    /// </summary>
    public interface IRenderTarget
    {
        internal NativeHandle Handle { get; }
        /// <summary>
        /// The framebuffer format.
        /// </summary>
        FramebufferFormat Format { get; }
        /// <summary>
        /// The width.
        /// </summary>
        uint Width { get; }
        /// <summary>
        /// The height.
        /// </summary>
        uint Height { get; }
    }
}