using AdvancedDLSupport;
using DigBuildPlatformCS.Resource;
using DigBuildPlatformCS.Util;
using System;

namespace DigBuildPlatformCS
{
    [NativeSymbols("dbp_render_context_", SymbolTransformationMethod.Underscore)]
    internal interface IRenderContextBindings
    {
        IntPtr CreateFramebuffer(IntPtr instance, IntPtr format, uint width, uint height);

        IntPtr CreateShader(
            IntPtr instance,
            ShaderType type,
            IntPtr data, int dataLength,
            IntPtr bindings, int bindingCount,
            IntPtr properties
        );

        IntPtr CreateRenderPipeline(
            IntPtr instance,
            IntPtr format, uint renderStage,
            IntPtr vertexFormat, uint vertexFormatLength,
            IntPtr instanceFormat, uint instanceFormatLength,
            IntPtr blendOptions,
            IntPtr vertexShader,
            IntPtr fragmentShader,
            Topology topology,
            RasterMode rasterMode,
            bool discardRaster,
            bool hasLineWidth, float lineWidth,
            bool hasDepthBias, DepthBias depthBias,
            bool hasDepthTest, DepthTest depthTest,
            bool hasStencilTest, StencilTest stencilTest,
            bool hasCullingMode, CullingMode cullingMode,
            bool hasFrontFace, FrontFace frontFace
        );

        IntPtr CreateVertexBuffer(
            IntPtr instance,
            IntPtr data, uint dataLength,
            int vertexSize,
            bool writable
        );

        IntPtr CreateCommandBuffer(IntPtr instance);

        void Enqueue(IntPtr instance, IntPtr renderTarget, IntPtr commandBuffer);
    }

    public readonly ref struct RenderContext
    {
        internal static readonly IRenderContextBindings Bindings = NativeLib.Get<IRenderContextBindings>();

        internal readonly IntPtr Ptr;

        internal RenderContext(IntPtr ptr)
        {
            Ptr = ptr;
        }

        public FramebufferFormatBuilder CreateFramebufferFormat(
        ) => new(this);

        public FramebufferBuilder CreateFramebuffer(
            FramebufferFormat format,
            uint width, uint height
        ) => new(this, format, width, height);

        public ShaderBuilder<VertexShader> CreateVertexShader(
            IResource resource
        ) => new(this, resource, ShaderType.Vertex, null, h => new VertexShader(h));

        public ShaderBuilder<VertexShader<TUniform>> CreateVertexShader<TUniform>(
            IResource resource
        ) where TUniform : class, IUniform<TUniform>
            => new(this, resource, ShaderType.Vertex, UniformDescriptor<TUniform>.Instance,
                h => new VertexShader<TUniform>(h));

        public ShaderBuilder<FragmentShader> CreateFragmentShader(
            IResource resource
        ) => new(this, resource, ShaderType.Fragment, null, h => new FragmentShader(h));

        public ShaderBuilder<FragmentShader<TUniform>> CreateFragmentShader<TUniform>(
            IResource resource
        ) where TUniform : class, IUniform<TUniform>
            => new(this, resource, ShaderType.Fragment, UniformDescriptor<TUniform>.Instance,
                h => new FragmentShader<TUniform>(h));

        public RenderPipelineBuilder<RenderPipeline<TVertex>> CreatePipeline<TVertex>(
            RenderStage renderStage,
            Topology topology,
            RasterMode rasterMode = RasterMode.Fill,
            bool discardRaster = false,
            MaybeDynamic<float>? lineWidth = null!,
            MaybeDynamic<DepthBias>? depthBias = null!,
            MaybeDynamic<DepthTest>? depthTest = null!,
            MaybeDynamic<StencilTest>? stencilTest = null!,
            MaybeDynamic<CullingMode>? cullingMode = null!,
            MaybeDynamic<FrontFace>? frontFace = null!
        ) where TVertex : unmanaged
            => new(
                this,
                renderStage,
                new RenderState(
                    topology, rasterMode, discardRaster,
                    lineWidth, depthBias, depthTest, stencilTest,
                    cullingMode, frontFace
                ),
                FormatDescriptor<TVertex>.Instance,
                null,
                handle => new RenderPipeline<TVertex>(handle)
            );

        public RenderPipelineBuilder<RenderPipeline<TVertex, TInstance>> CreatePipeline<TVertex, TInstance>(
            RenderStage renderStage,
            Topology topology,
            RasterMode rasterMode = RasterMode.Fill,
            bool discardRaster = false,
            MaybeDynamic<float>? lineWidth = null!,
            MaybeDynamic<DepthBias>? depthBias = null!,
            MaybeDynamic<DepthTest>? depthTest = null!,
            MaybeDynamic<StencilTest>? stencilTest = null!,
            MaybeDynamic<CullingMode>? cullingMode = null!,
            MaybeDynamic<FrontFace>? frontFace = null!
        ) where TVertex : unmanaged where TInstance : unmanaged
            => new(
                this,
                renderStage,
                new RenderState(
                    topology, rasterMode, discardRaster,
                    lineWidth, depthBias, depthTest, stencilTest,
                    cullingMode, frontFace
                ),
                FormatDescriptor<TVertex>.Instance,
                FormatDescriptor<TInstance>.Instance,
                handle => new RenderPipeline<TVertex, TInstance>(handle)
            );

        public VertexBufferBuilder<TVertex> CreateVertexBuffer<TVertex>(
            NativeBuffer<TVertex> initialData
        ) where TVertex : unmanaged
            => new(this, initialData);

        public VertexBufferBuilder<TVertex> CreateVertexBuffer<TVertex>(
            out VertexBufferWriter<TVertex> writer,
            NativeBuffer<TVertex>? initialData = null
        ) where TVertex : unmanaged
            => new(this, initialData, out writer);

        public VertexBufferBuilder<TVertex> CreateVertexBuffer<TVertex>(
            PooledNativeBuffer<TVertex> initialData
        ) where TVertex : unmanaged
            => CreateVertexBuffer(initialData.Unpooled);

        public VertexBufferBuilder<TVertex> CreateVertexBuffer<TVertex>(
            out VertexBufferWriter<TVertex> writer,
            PooledNativeBuffer<TVertex> initialData
        ) where TVertex : unmanaged
            => CreateVertexBuffer(out writer, initialData.Unpooled);

        public UniformBuffer<TUniform> CreateUniformBuffer<TUniform>(
            Shader<TUniform> shader
        ) where TUniform : class, IUniform<TUniform>
        {
            throw new NotImplementedException();
        }

        public Texture CreateTexture(
        ) => throw new NotImplementedException();

        public CommandBufferBuilder CreateDrawCommand(
            out CommandBufferWriter writer
        ) => new(this, out writer);

        public void Enqueue(
            IRenderTarget target,
            CommandBuffer cmd
        ) => Bindings.Enqueue(Ptr, target.Handle, cmd.Handle);
    }
}