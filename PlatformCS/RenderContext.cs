using System;

namespace DigBuildPlatformCS
{
    public readonly ref struct RenderContext
    {
        internal bool Valid => throw new NotImplementedException();

        public FramebufferBuilder CreateFramebuffer(
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

        public RenderPipelineBuilder<TVertex> CreatePipeline<TVertex>(
            Topology topology,
            RasterMode rasterMode = RasterMode.Fill,
            bool discardRaster = false
        ) where TVertex : unmanaged
            => throw new NotImplementedException();

        public VertexBufferBuilder<TVertex> CreateVertexBuffer<TVertex>(
            NativeBuffer<TVertex>? initialData = null
        ) where TVertex : unmanaged
            => throw new NotImplementedException();

        public DrawCommandBuilder CreateDrawCommand<TVertex>(
            RenderPipeline<TVertex> pipeline,
            params VertexBuffer<TVertex>[] vertexBuffers
        ) where TVertex : unmanaged
            => throw new NotImplementedException();
    }
}