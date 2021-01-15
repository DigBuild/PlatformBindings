using System;
using DigBuildPlatformCS.Util;

namespace DigBuildPlatformCS
{
    public readonly ref struct RenderContext
    {
        public FramebufferFormatBuilder CreateFramebufferFormat(
        ) => throw new NotImplementedException();

        public FramebufferBuilder CreateFramebuffer(
            FramebufferFormat format,
            uint width, uint height
        ) => throw new NotImplementedException();

        public ShaderBuilder<VertexShader> CreateVertexShader(
            // ...
        ) => throw new NotImplementedException();
        public ShaderBuilder<VertexShader<TUniform>> CreateVertexShader<TUniform>(
            // ...
        ) => throw new NotImplementedException();

        public ShaderBuilder<FragmentShader> CreateFragmentShader(
            // ...
        ) => throw new NotImplementedException();
        public ShaderBuilder<FragmentShader<TUniform>> CreateFragmentShader<TUniform>(
            // ...
        ) => throw new NotImplementedException();

        public RenderPipelineBuilder<RenderPipeline<TVertex>> CreatePipeline<TVertex>(
            RenderStage renderStage,
            Topology topology,
            RasterMode rasterMode = RasterMode.Fill,
            bool discardRaster = false
        ) where TVertex : unmanaged
            => throw new NotImplementedException();

        public RenderPipelineBuilder<RenderPipeline<TVertex, TInstance>> CreatePipeline<TVertex, TInstance>(
            RenderStage renderStage,
            Topology topology,
            RasterMode rasterMode = RasterMode.Fill,
            bool discardRaster = false
        ) where TVertex : unmanaged where TInstance : unmanaged
            => throw new NotImplementedException();

        public VertexBufferBuilder<TVertex> CreateVertexBuffer<TVertex>(
            NativeBuffer<TVertex> initialData
        ) where TVertex : unmanaged
            => throw new NotImplementedException();

        public VertexBufferBuilder<TVertex> CreateVertexBuffer<TVertex>(
            out VertexBufferWriter<TVertex> writer,
            NativeBuffer<TVertex>? initialData = null
        ) where TVertex : unmanaged
            => throw new NotImplementedException();

        public VertexBufferBuilder<TVertex> CreateVertexBuffer<TVertex>(
            PooledNativeBuffer<TVertex> initialData
        ) where TVertex : unmanaged
            => CreateVertexBuffer(initialData.Unpooled);

        public VertexBufferBuilder<TVertex> CreateVertexBuffer<TVertex>(
            out VertexBufferWriter<TVertex> writer,
            PooledNativeBuffer<TVertex> initialData
        ) where TVertex : unmanaged
            => CreateVertexBuffer(out writer, initialData.Unpooled);

        public DrawCommandBuilder CreateDrawCommand(
        ) => throw new NotImplementedException();

        public void Enqueue(
            DrawCommand command
        ) => throw new NotImplementedException();
    }
}