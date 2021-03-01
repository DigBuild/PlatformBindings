﻿using System;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Render
{
    public sealed class RenderPipeline<TVertex> : IRenderPipeline
        where TVertex : unmanaged
    {
        internal readonly NativeHandle Handle;
        NativeHandle IRenderPipeline.Handle => Handle;

        internal RenderPipeline(NativeHandle handle)
        {
            Handle = handle;
        }
    }

    public sealed class RenderPipeline<TVertex, TInstance> : IRenderPipeline
        where TVertex : unmanaged
        where TInstance : unmanaged
    {
        internal readonly NativeHandle Handle;
        NativeHandle IRenderPipeline.Handle => Handle;

        internal RenderPipeline(NativeHandle handle)
        {
            Handle = handle;
        }
    }

    public interface IRenderPipeline {
        internal NativeHandle Handle { get; }
    }

    public readonly ref struct RenderPipelineBuilder<TPipeline> where TPipeline : IRenderPipeline
    {
        private class Data
        {
            internal readonly VertexShader VertexShader;
            internal readonly FragmentShader FragmentShader;
            internal readonly RenderStage RenderStage;
            internal readonly RenderState RenderState;
            internal readonly FormatDescriptor VertexDescriptor;
            internal readonly FormatDescriptor? InstanceDescriptor;
            internal readonly Func<NativeHandle, TPipeline> Factory;
            internal readonly BlendOptions[] BlendOptions;

            internal Data(
                VertexShader vertexShader,
                FragmentShader fragmentShader,
                RenderStage renderStage,
                RenderState renderState,
                FormatDescriptor vertexDescriptor,
                FormatDescriptor? instanceDescriptor,
                Func<NativeHandle, TPipeline> factory
            )
            {
                VertexShader = vertexShader;
                FragmentShader = fragmentShader;
                RenderStage = renderStage;
                RenderState = renderState;
                VertexDescriptor = vertexDescriptor;
                InstanceDescriptor = instanceDescriptor;
                Factory = factory;
                BlendOptions = new BlendOptions[renderStage.Format.Attachments.Count];
                for (var i = 0; i < BlendOptions.Length; i++)
                    BlendOptions[i] = new BlendOptions(true);
            }
        }

        private readonly RenderContext _context;
        private readonly Data _data;

        internal RenderPipelineBuilder(
            RenderContext context,
            VertexShader vertexShader,
            FragmentShader fragmentShader,
            RenderStage renderStage,
            RenderState renderState,
            FormatDescriptor vertexDescriptor,
            FormatDescriptor? instanceDescriptor,
            Func<NativeHandle, TPipeline> factory
        )
        {
            _context = context;
            _data = new Data(
                vertexShader, fragmentShader,
                renderStage, renderState,
                vertexDescriptor, instanceDescriptor,
                factory
            );
        }

        public RenderPipelineBuilder<TPipeline> WithStandardBlending(
            FramebufferAttachment attachment
        ) => WithBlending(
            attachment,
            BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha, BlendOperation.Add
        );

        public RenderPipelineBuilder<TPipeline> WithPreMultipliedBlending(
            FramebufferAttachment attachment
        ) => WithBlending(
            attachment,
            BlendFactor.One, BlendFactor.OneMinusSrcAlpha, BlendOperation.Add
        );

        public RenderPipelineBuilder<TPipeline> WithPreMultipliedInverseBlending(
            FramebufferAttachment attachment
        ) => WithBlending(
            attachment,
            BlendFactor.One, BlendFactor.SrcAlpha, BlendOperation.Add
        );

        public RenderPipelineBuilder<TPipeline> WithBlending(
            FramebufferAttachment attachment,
            BlendFactor src, BlendFactor dst, BlendOperation operation,
            ColorComponent components =
                ColorComponent.Red | ColorComponent.Green | ColorComponent.Blue | ColorComponent.Alpha
        ) => WithBlending(
            attachment,
            src, dst, operation,
            src, dst, operation,
            components
        );

        public RenderPipelineBuilder<TPipeline> WithBlending(
            FramebufferAttachment attachment,
            BlendFactor srcColor, BlendFactor dstColor, BlendOperation colorOperation,
            BlendFactor srcAlpha, BlendFactor dstAlpha, BlendOperation alphaOperation,
            ColorComponent components =
                ColorComponent.Red | ColorComponent.Green | ColorComponent.Blue | ColorComponent.Alpha
        )
        {
            ref var options = ref _data.BlendOptions[attachment.Id];

            options.Enabled = true;
            options.SrcColor = srcColor;
            options.DstColor = dstColor;
            options.ColorOperation = colorOperation;
            options.SrcAlpha = srcAlpha;
            options.DstAlpha = dstAlpha;
            options.AlphaOperation = alphaOperation;
            options.Components = components;

            return this;
        }

        public static unsafe implicit operator TPipeline(RenderPipelineBuilder<TPipeline> builder)
        {
            var data = builder._data;

            var span1 = new Span<FormatDescriptor.Element>(data.VertexDescriptor.Elements);
            var span2 = data.InstanceDescriptor != null ?
                new Span<FormatDescriptor.Element>(data.InstanceDescriptor.Elements) :
                Span<FormatDescriptor.Element>.Empty;
            var span3 = new Span<BlendOptions>(data.BlendOptions);

            fixed (FormatDescriptor.Element* p1 = &span1.GetPinnableReference())
            fixed (FormatDescriptor.Element* p2 = &span2.GetPinnableReference())
            fixed (BlendOptions* p3 = &span3.GetPinnableReference())
            {
                var handle = new NativeHandle(
                    RenderContext.Bindings.CreateRenderPipeline(
                        builder._context.Ptr,
                        data.RenderStage.Format.Handle,
                        data.RenderStage.Id,
                        new IntPtr(p1), (uint) span1.Length,
                        new IntPtr(p2), (uint) span2.Length,
                        new IntPtr(p3),
                        data.VertexShader.Handle,
                        data.FragmentShader.Handle,
                        data.RenderState.Topology,
                        data.RenderState.RasterMode,
                        data.RenderState.DiscardRaster,
                        data.RenderState.LineWidth.HasValue,
                        data.RenderState.LineWidth.Value,
                        data.RenderState.DepthBias.HasValue,
                        data.RenderState.DepthBias.Value,
                        data.RenderState.DepthTest.HasValue,
                        data.RenderState.DepthTest.Value,
                        data.RenderState.StencilTest.HasValue,
                        data.RenderState.StencilTest.Value,
                        data.RenderState.CullingMode.HasValue,
                        data.RenderState.CullingMode.Value,
                        data.RenderState.FrontFace.HasValue,
                        data.RenderState.FrontFace.Value
                    )
                );
                return data.Factory(handle);
            }
        }

        private struct BlendOptions
        {
            internal bool Enabled;
            internal BlendFactor SrcColor, DstColor;
            internal BlendOperation ColorOperation;
            internal BlendFactor SrcAlpha, DstAlpha;
            internal BlendOperation AlphaOperation;
            internal ColorComponent Components;

            public BlendOptions(bool _) : this()
            {
                SrcColor = SrcAlpha = BlendFactor.One;
                DstColor = DstAlpha = BlendFactor.Zero;
                ColorOperation = AlphaOperation = BlendOperation.Add;
                Components = ColorComponent.Red | ColorComponent.Green | ColorComponent.Blue | ColorComponent.Alpha;
            }
        }
    }
}