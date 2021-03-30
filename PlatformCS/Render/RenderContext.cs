using System;
using System.Drawing;
using System.Drawing.Imaging;
using AdvancedDLSupport;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Render
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
        IntPtr CreateTexture(
            IntPtr instance,
            uint width, uint height,
            IntPtr dataStart, uint dataLength
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
            bool discardRaster = false
        ) where TVertex : unmanaged
            => new(
                this,
                vertexShader, fragmentShader,
                renderStage,
                topology, rasterMode, discardRaster,
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
            bool discardRaster = false
        ) where TVertex : unmanaged where TInstance : unmanaged
            => new(
                this,
                vertexShader, fragmentShader,
                renderStage,
                topology, rasterMode, discardRaster,
                FormatDescriptor<TVertex>.Instance,
                FormatDescriptor<TInstance>.Instance,
                handle => new RenderPipeline<TVertex, TInstance>(handle)
            );

        public VertexBufferBuilder<TVertex> CreateVertexBuffer<TVertex>(
            INativeBuffer<TVertex> initialData
        ) where TVertex : unmanaged
            => new(this, initialData);

        public VertexBufferBuilder<TVertex> CreateVertexBuffer<TVertex>(
            out VertexBufferWriter<TVertex> writer,
            INativeBuffer<TVertex>? initialData = null
        ) where TVertex : unmanaged
            => new(this, initialData, out writer);

        public UniformBuffer<TUniform> CreateUniformBuffer<TUniform>(
            UniformHandle<TUniform> uniform
        ) where TUniform : unmanaged, IUniform<TUniform>
            => new UniformBufferBuilder<TUniform>(this, uniform, null);

        public UniformBuffer<TUniform> CreateUniformBuffer<TUniform>(
            UniformHandle<TUniform> uniform,
            INativeBuffer<TUniform> initialData
        ) where TUniform : unmanaged, IUniform<TUniform>
            => new UniformBufferBuilder<TUniform>(this, uniform, initialData);

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
            Bitmap image
        )
        {
            var data = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb
            );
            var length = (uint) (Math.Abs(data.Stride) * image.Height);

            var texture = new Texture(new NativeHandle(
                Bindings.CreateTexture(
                    Ptr,
                    (uint) image.Width, (uint)image.Height,
                    data.Scan0, length
                )
            ));
            
            image.UnlockBits(data);

            return texture;
        }

        public CommandBufferBuilder CreateCommandBuffer(
        ) => new(this);

        public void Enqueue(
            IRenderTarget target,
            CommandBuffer cmd
        ) => Bindings.Enqueue(Ptr, target.Handle, cmd.Handle);
    }
}