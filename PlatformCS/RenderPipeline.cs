using System;

namespace DigBuildPlatformCS
{
    public interface IRenderPipeline { }

    public sealed class RenderPipeline<TVertex> : IRenderPipeline
        where TVertex : unmanaged
    {
    }
    public sealed class RenderPipeline<TVertex, TInstance> : IRenderPipeline
        where TVertex : unmanaged
        where TInstance : unmanaged
    {
    }

    public readonly ref struct RenderPipelineBuilder<TPipeline> where TPipeline : IRenderPipeline
    {
        public RenderPipelineBuilder<TPipeline> WithShader<TUniform>(
            VertexShader<TUniform> shader,
            out TUniform uniform
        ) => throw new NotImplementedException();
        public RenderPipelineBuilder<TPipeline> WithShader(
            VertexShader shader
        ) => throw new NotImplementedException();

        public RenderPipelineBuilder<TPipeline> WithShader<TUniform>(
            FragmentShader<TUniform> shader,
            out TUniform uniform
        ) => throw new NotImplementedException();
        public RenderPipelineBuilder<TPipeline> WithShader(
            FragmentShader shader
        ) => throw new NotImplementedException();

        public RenderPipelineBuilder<TPipeline> WithStandardBlending(
            uint attachment
        ) => WithBlending(
            attachment,
            BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha, BlendOperation.Add
        );

        public RenderPipelineBuilder<TPipeline> WithPreMultipliedBlending(
            uint attachment
        ) => WithBlending(
            attachment,
            BlendFactor.One, BlendFactor.OneMinusSrcAlpha, BlendOperation.Add
        );

        public RenderPipelineBuilder<TPipeline> WithPreMultipliedInverseBlending(
            uint attachment
        ) => WithBlending(
            attachment,
            BlendFactor.One, BlendFactor.SrcAlpha, BlendOperation.Add
        );

        public RenderPipelineBuilder<TPipeline> WithBlending(
            uint attachment,
            BlendFactor src, BlendFactor dst, BlendOperation operation,
            ColorComponent components = ColorComponent.Red | ColorComponent.Green | ColorComponent.Blue | ColorComponent.Alpha
        ) => WithBlending(
            attachment,
            src, dst, operation,
            src, dst, operation,
            components
        );

        public RenderPipelineBuilder<TPipeline> WithBlending(
            uint attachment,
            BlendFactor srcColor, BlendFactor dstColor, BlendOperation colorOperation,
            BlendFactor srcAlpha, BlendFactor dstAlpha, BlendOperation alphaOperation,
            ColorComponent components = ColorComponent.Red | ColorComponent.Green | ColorComponent.Blue | ColorComponent.Alpha
        ) => throw new NotImplementedException();

        public static implicit operator TPipeline(RenderPipelineBuilder<TPipeline> builder) => throw new NotImplementedException();
    }
}