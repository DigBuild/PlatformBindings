using System;

namespace DigBuildPlatformCS
{
    public sealed class RenderPipeline<TVertex> where TVertex : unmanaged
    {
    }

    public readonly ref struct RenderPipelineBuilder<TVertex> where TVertex : unmanaged
    {
        public RenderPipelineBuilder<TVertex> With<TUniform>(
            VertexShader<TUniform> shader,
            out TUniform uniform
        ) => throw new NotImplementedException();
        public RenderPipelineBuilder<TVertex> With(
            VertexShader shader
        ) => throw new NotImplementedException();

        public RenderPipelineBuilder<TVertex> With<TUniform>(
            FragmentShader<TUniform> shader,
            out TUniform uniform
        ) => throw new NotImplementedException();
        public RenderPipelineBuilder<TVertex> With(
            FragmentShader shader
        ) => throw new NotImplementedException();

        public static implicit operator RenderPipeline<TVertex>(RenderPipelineBuilder<TVertex> builder) => throw new NotImplementedException();
    }
}