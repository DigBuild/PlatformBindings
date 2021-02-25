using AdvancedDLSupport;
using DigBuildPlatformCS.Resource;
using DigBuildPlatformCS.Util;
using System;

namespace DigBuildPlatformCS
{
    [NativeSymbols("dbp_render_context_", SymbolTransformationMethod.Underscore)]
    internal interface IRenderContextBindings
    {
        IntPtr CreateFramebufferFormat(
            IntPtr instance,
            IntPtr attachments, uint attachmentCount,
            IntPtr stages, uint stageCount,
            IntPtr members,
            IntPtr dependencies
        );

        IntPtr CreateFramebuffer(IntPtr instance, IntPtr format, uint width, uint height);

        IntPtr CreateShader(
            IntPtr instance,
            ShaderType type,
            IntPtr data, int dataLength,
            IntPtr bindings, int bindingCount,
            IntPtr members
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

        IntPtr CreateUniformBuffer(
            IntPtr instance,
            IntPtr shader,
            uint binding,
            IntPtr data, uint dataLength
        );

        IntPtr CreateVertexBuffer(
            IntPtr instance,
            IntPtr data, uint dataLength,
            int vertexSize,
            bool writable
        );

        IntPtr CreateTextureSampler(
            IntPtr instance,
            TextureFiltering minFiltering,
            TextureFiltering maxFiltering,
            TextureWrapping wrapping,
            TextureBorderColor borderColor,
            bool enableAnisotropy,
            uint anisotropyLevel
        );
        IntPtr CreateTextureBinding(
            IntPtr instance,
            IntPtr shader, uint binding,
            IntPtr sampler,
            IntPtr texture
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
        ) => new(this, resource, ShaderType.Vertex, h => new VertexShader(h));
        
        public ShaderBuilder<FragmentShader> CreateFragmentShader(
            IResource resource
        ) => new(this, resource, ShaderType.Fragment, h => new FragmentShader(h));
        
        public RenderPipelineBuilder<RenderPipeline<TVertex>> CreatePipeline<TVertex>(
            VertexShader vertexShader,
            FragmentShader fragmentShader,
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
                vertexShader, fragmentShader,
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
            VertexShader vertexShader,
            FragmentShader fragmentShader,
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
                vertexShader, fragmentShader,
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
            UniformHandle<TUniform> uniform
        ) where TUniform : unmanaged, IUniform<TUniform>
            => new UniformBufferBuilder<TUniform>(this, uniform, null);

        public UniformBuffer<TUniform> CreateUniformBuffer<TUniform>(
            UniformHandle<TUniform> uniform,
            NativeBuffer<TUniform> initialData
        ) where TUniform : unmanaged, IUniform<TUniform>
            => new UniformBufferBuilder<TUniform>(this, uniform, initialData);

        public UniformBuffer<TUniform> CreateUniformBuffer<TUniform>(
            UniformHandle<TUniform> uniform,
            PooledNativeBuffer<TUniform> initialData
        ) where TUniform : unmanaged, IUniform<TUniform>
            => CreateUniformBuffer(uniform, initialData.Unpooled);

        public TextureSamplerBuilder CreateTextureSampler(
            TextureFiltering minFiltering = TextureFiltering.Linear,
            TextureFiltering maxFiltering = TextureFiltering.Linear,
            TextureWrapping wrapping = TextureWrapping.Repeat,
            TextureBorderColor borderColor = TextureBorderColor.OpaqueBlack
        ) => new(this, minFiltering, maxFiltering, wrapping, borderColor);

        public TextureBindingBuilder CreateTextureBinding(
            ShaderSamplerHandle shaderSampler,
            TextureSampler sampler,
            Texture texture
        ) => new(this, shaderSampler, sampler, texture);

        public Texture CreateTexture(
        ) => throw new NotImplementedException();

        public CommandBufferBuilder CreateCommandBuffer(
        ) => new(this);

        public void Enqueue(
            IRenderTarget target,
            CommandBuffer cmd
        ) => Bindings.Enqueue(Ptr, target.Handle, cmd.Handle);
    }
}