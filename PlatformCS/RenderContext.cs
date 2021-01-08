using System;

namespace DigBuildPlatformCS
{
    public readonly ref struct RenderContext
    {
        internal bool Valid => throw new NotImplementedException();

        public FramebufferBuilder CreateFramebuffer(
            uint width, uint height
        ) => throw new NotImplementedException();

        public ShaderBuilder<VertexShader<TUniform>> CreateVertexShader<TUniform>(
            // ...
        ) => throw new NotImplementedException();

        public ShaderBuilder<FragmentShader<TUniform>> CreateFragmentShader<TUniform>(
            // ...
        ) => throw new NotImplementedException();

        public RenderPipelineBuilder CreatePipeline(
            // ...
        ) => throw new NotImplementedException();
    }
}