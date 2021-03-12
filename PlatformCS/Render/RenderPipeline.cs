using System;
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

            internal readonly Topology Topology;
            internal readonly RasterMode RasterMode;
            internal readonly bool DiscardRaster;
            internal MaybeDynamic<float> LineWidth = 1.0f;
            internal MaybeDynamic<DepthBias> DepthBias = Render.DepthBias.Default;
            internal MaybeDynamic<DepthTest> DepthTest = Render.DepthTest.Default;
            internal MaybeDynamic<StencilTest> StencilTest = Render.StencilTest.Default;
            internal MaybeDynamic<CullingMode> CullingMode = Render.CullingMode.Back;
            internal MaybeDynamic<FrontFace> FrontFace = Render.FrontFace.Clockwise;

            internal readonly FormatDescriptor VertexDescriptor;
            internal readonly FormatDescriptor? InstanceDescriptor;
            internal readonly Func<NativeHandle, TPipeline> Factory;
            internal readonly BlendOptions[] BlendOptions;

            internal Data(
                VertexShader vertexShader,
                FragmentShader fragmentShader,
                RenderStage renderStage,
                Topology topology,
                RasterMode rasterMode,
                bool discardRaster,
                FormatDescriptor vertexDescriptor,
                FormatDescriptor? instanceDescriptor,
                Func<NativeHandle, TPipeline> factory
            )
            {
                VertexShader = vertexShader;
                FragmentShader = fragmentShader;
                RenderStage = renderStage;
                Topology = topology;
                RasterMode = rasterMode;
                DiscardRaster = discardRaster;
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
            Topology topology,
            RasterMode rasterMode,
            bool discardRaster,
            FormatDescriptor vertexDescriptor,
            FormatDescriptor? instanceDescriptor,
            Func<NativeHandle, TPipeline> factory
        )
        {
            _context = context;
            _data = new Data(
                vertexShader, fragmentShader,
                renderStage,
                topology, rasterMode, discardRaster,
                vertexDescriptor, instanceDescriptor,
                factory
            );
        }

        public RenderPipelineBuilder<TPipeline> WithLineWidth(float lineWidth)
        {
            _data.LineWidth = lineWidth;
            return this;
        }
        public RenderPipelineBuilder<TPipeline> WithDynamicLineWidth()
        {
            _data.LineWidth = MaybeDynamic<float>.Dynamic;
            return this;
        }

        public RenderPipelineBuilder<TPipeline> WithDepthBias(float constant, float clamp, float slope)
        {
            _data.DepthBias = new DepthBias(true, constant, clamp, slope);
            return this;
        }
        public RenderPipelineBuilder<TPipeline> WithDynamicDepthBias()
        {
            _data.DepthBias = MaybeDynamic<DepthBias>.Dynamic;
            return this;
        }

        public RenderPipelineBuilder<TPipeline> WithDepthTest(CompareOperation comparison, bool write)
        {
            _data.DepthTest = new DepthTest(true, comparison, write);
            return this;
        }
        public RenderPipelineBuilder<TPipeline> WithDynamicDepthTest()
        {
            _data.DepthTest = MaybeDynamic<DepthTest>.Dynamic;
            return this;
        }
        
        public RenderPipelineBuilder<TPipeline> WithStencilTest(StencilFaceOperation operation)
        {
            return WithStencilTest(operation, operation);
        }
        public RenderPipelineBuilder<TPipeline> WithStencilTest(StencilFaceOperation front, StencilFaceOperation back)
        {
            _data.StencilTest = new StencilTest(true, front, back);
            return this;
        }
        public RenderPipelineBuilder<TPipeline> WithDynamicStencilTest()
        {
            _data.StencilTest = MaybeDynamic<StencilTest>.Dynamic;
            return this;
        }

        public RenderPipelineBuilder<TPipeline> WithCullingMode(CullingMode cullingMode)
        {
            _data.CullingMode = cullingMode;
            return this;
        }
        public RenderPipelineBuilder<TPipeline> WithDynamicCullingMode()
        {
            _data.CullingMode = MaybeDynamic<CullingMode>.Dynamic;
            return this;
        }

        public RenderPipelineBuilder<TPipeline> WithFrontFace(FrontFace frontFace)
        {
            _data.FrontFace = frontFace;
            return this;
        }
        public RenderPipelineBuilder<TPipeline> WithDynamicFrontFace()
        {
            _data.FrontFace = MaybeDynamic<FrontFace>.Dynamic;
            return this;
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
                        data.Topology,
                        data.RasterMode,
                        data.DiscardRaster,
                        data.LineWidth.HasValue,
                        data.LineWidth.Value,
                        data.DepthBias.HasValue,
                        data.DepthBias.Value,
                        data.DepthTest.HasValue,
                        data.DepthTest.Value,
                        data.StencilTest.HasValue,
                        data.StencilTest.Value,
                        data.CullingMode.HasValue,
                        data.CullingMode.Value,
                        data.FrontFace.HasValue,
                        data.FrontFace.Value
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