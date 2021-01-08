using System;

namespace DigBuildPlatformCS
{
    public sealed class RenderPipeline
    {
    }

    public readonly ref struct RenderPipelineBuilder
    {
        public RenderPipelineBuilder With<TUniform>(
            VertexShader<TUniform> shader,
            out TUniform uniform
        ) => throw new NotImplementedException();

        public static implicit operator RenderPipeline(RenderPipelineBuilder builder) => throw new NotImplementedException();
    }
}