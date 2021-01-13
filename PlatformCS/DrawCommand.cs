using System;
using System.Numerics;

namespace DigBuildPlatformCS
{

    public sealed class DrawCommand
    {
        public DrawCommandRecorder BeginRecording() => throw new NotImplementedException();
    }

    public sealed class DrawCommandRecorder : IDisposable
    {
        public DrawCommandRecorderSetTargetOptions SetAndClearRenderTarget(
            Framebuffer framebuffer
        ) => throw new NotImplementedException();

        public void Draw<TVertex>(
            RenderPipeline<TVertex> pipeline,
            VertexBuffer<TVertex> vertexBuffer
        ) where TVertex : unmanaged
            => throw new NotImplementedException();

        public void Draw<TVertex, TInstance>(
            RenderPipeline<TVertex, TInstance> pipeline,
            VertexBuffer<TVertex> vertexBuffer,
            VertexBuffer<TInstance> instanceBuffer
        ) where TVertex : unmanaged where TInstance : unmanaged
            => throw new NotImplementedException();

        void IDisposable.Dispose() => Done();

        public void Done() => throw new NotImplementedException();
    }

    public readonly ref struct DrawCommandRecorderSetTargetOptions
    {
        public DrawCommandRecorderSetTargetOptions WithColor(
            FramebufferAttachment attachment,
            Vector4 clearColor
        ) => throw new NotImplementedException();
    }

    public readonly ref struct DrawCommandBuilder
    {
        // public DrawCommandBuilder WithRenderTarget(
        //     Framebuffer framebuffer
        // ) => throw new NotImplementedException();
        //
        // public DrawCommandBuilder WithColor(
        //     FramebufferAttachment attachment,
        //     Vector4 clearColor
        // ) => throw new NotImplementedException();
        //
        // public DrawCommandBuilder With<TVertex>(
        //     RenderPipeline<TVertex> pipeline,
        //     VertexBuffer<TVertex> vertexBuffer
        // ) where TVertex : unmanaged
        //     => throw new NotImplementedException();
        //
        // public DrawCommandBuilder With<TVertex, TInstance>(
        //     RenderPipeline<TVertex, TInstance> pipeline,
        //     VertexBuffer<TVertex> vertexBuffer,
        //     VertexBuffer<TInstance> instanceBuffer
        // ) where TVertex : unmanaged where TInstance : unmanaged
        //     => throw new NotImplementedException();

        public static implicit operator DrawCommand(DrawCommandBuilder builder) => throw new NotImplementedException();
    }
}