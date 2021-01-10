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

        public RenderPipelineBuilder<TVertex> WithStandardBlending(
            uint attachment
        ) => WithBlending(
            attachment,
            BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha, BlendOperation.Add
        );

        public RenderPipelineBuilder<TVertex> WithPreMultipliedBlending(
            uint attachment
        ) => WithBlending(
            attachment,
            BlendFactor.One, BlendFactor.OneMinusSrcAlpha, BlendOperation.Add
        );

        public RenderPipelineBuilder<TVertex> WithPreMultipliedInverseBlending(
            uint attachment
        ) => WithBlending(
            attachment,
            BlendFactor.One, BlendFactor.SrcAlpha, BlendOperation.Add
        );

        public RenderPipelineBuilder<TVertex> WithBlending(
            uint attachment,
            BlendFactor src, BlendFactor dst, BlendOperation operation,
            ColorComponent components = ColorComponent.Red | ColorComponent.Green | ColorComponent.Blue | ColorComponent.Alpha
        ) => WithBlending(
            attachment,
            src, dst, operation,
            src, dst, operation,
            components
        );

        public RenderPipelineBuilder<TVertex> WithBlending(
            uint attachment,
            BlendFactor srcColor, BlendFactor dstColor, BlendOperation colorOperation,
            BlendFactor srcAlpha, BlendFactor dstAlpha, BlendOperation alphaOperation,
            ColorComponent components = ColorComponent.Red | ColorComponent.Green | ColorComponent.Blue | ColorComponent.Alpha
        ) => throw new NotImplementedException();

        public static implicit operator RenderPipeline<TVertex>(RenderPipelineBuilder<TVertex> builder) => throw new NotImplementedException();
    }
}