using System;

namespace DigBuildPlatformCS
{
    public readonly ref struct RenderContext
    {
        internal bool Valid => throw new NotImplementedException();

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

        public RenderPipelineBuilder<TVertex> CreatePipeline<TVertex>(
            RenderStage renderStage,
            Topology topology,
            RasterMode rasterMode = RasterMode.Fill,
            bool discardRaster = false
        ) where TVertex : unmanaged
            => throw new NotImplementedException();

        public VertexBufferBuilder<TVertex> CreateVertexBuffer<TVertex>(
            NativeBuffer<TVertex>? initialData = null
        ) where TVertex : unmanaged
            => throw new NotImplementedException();

        public DrawCommandBuilder CreateDrawCommand(
        ) => throw new NotImplementedException();

        public void Enqueue(
            DrawCommand command
        ) => throw new NotImplementedException();
    }
}