using System;
using System.Numerics;

namespace DigBuildPlatformCS
{
    public class FramebufferFormat
    {

    }

    public readonly struct RenderStage
    {

    }

    public readonly ref struct FramebufferFormatBuilder
    {
        public FramebufferFormatBuilder WithColorAttachment(
            out FramebufferAttachment attachment,
            TextureFormat format,
            Vector4 clearColor
        ) => throw new NotImplementedException();

        public FramebufferFormatBuilder WithDepthStencilAttachment(
            out FramebufferAttachment attachment,
            TextureFormat format
        ) => throw new NotImplementedException();

        public FramebufferFormatBuilder WithStage(
            out RenderStage stage,
            params FramebufferAttachment[] attachments
        ) => throw new NotImplementedException();

        public FramebufferFormatBuilder WithDependency(
            RenderStage stage,
            params RenderStage[] dependencies
        ) => throw new NotImplementedException();

        public static implicit operator FramebufferFormat(FramebufferFormatBuilder builder)
            => throw new NotImplementedException();
    }
}