using System;
using System.Numerics;

namespace DigBuildPlatformCS
{
    public sealed class DrawCommand
    {
        
    }

    public readonly ref struct DrawCommandBuilder
    {
        public DrawCommandBuilder WithRenderTarget(
            Framebuffer framebuffer
        ) => throw new NotImplementedException();

        public DrawCommandBuilder WithClearColor(
            FramebufferAttachment attachment,
            Vector4 clearColor
        ) => throw new NotImplementedException();

        public DrawCommandBuilder With<TVertex>(
            RenderPipeline<TVertex> pipeline,
            params VertexBuffer<TVertex>[] vertexBuffers
        ) where TVertex : unmanaged
            => throw new NotImplementedException();

        public static implicit operator DrawCommand(DrawCommandBuilder builder) => throw new NotImplementedException();
    }
}