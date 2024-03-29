﻿using System;
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
        
        IntPtr CreateUniformBinding(
            IntPtr instance,
            IntPtr shader, uint binding,
            IntPtr uniformBuffer
        );
        IntPtr CreateUniformBuffer(
            IntPtr instance,
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

    /// <summary>
    /// A render context.
    /// </summary>
    public readonly ref struct RenderContext
    {
        internal static readonly IRenderContextBindings Bindings = NativeLib.Get<IRenderContextBindings>();

        internal readonly IntPtr Ptr;

        internal RenderContext(IntPtr ptr)
        {
            Ptr = ptr;
        }

        /// <summary>
        /// Creates a new framebuffer format builder.
        /// </summary>
        /// <returns>The builder</returns>
        public FramebufferFormatBuilder CreateFramebufferFormat(
        ) => new(this);
        
        /// <summary>
        /// Creates a new framebuffer builder.
        /// </summary>
        /// <param name="format">The format</param>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        /// <returns>The builder</returns>
        public FramebufferBuilder CreateFramebuffer(
            FramebufferFormat format,
            uint width, uint height
        ) => new(this, format, width, height);

        /// <summary>
        /// Creates a new vertex shader builder.
        /// </summary>
        /// <param name="resource">The shader resource</param>
        /// <returns>The builder</returns>
        public ShaderBuilder<VertexShader> CreateVertexShader(
            IResource resource
        ) => new(this, resource, ShaderType.Vertex, h => new VertexShader(h));
        
        /// <summary>
        /// Creates a new fragment shader builder.
        /// </summary>
        /// <param name="resource">The shader resource</param>
        /// <returns>The builder</returns>
        public ShaderBuilder<FragmentShader> CreateFragmentShader(
            IResource resource
        ) => new(this, resource, ShaderType.Fragment, h => new FragmentShader(h));
        
        /// <summary>
        /// Creates a new render pipeline builder.
        /// </summary>
        /// <typeparam name="TVertex">The vertex format</typeparam>
        /// <param name="vertexShader">The vertex shader</param>
        /// <param name="fragmentShader">The fragment shader</param>
        /// <param name="renderStage">The render stage</param>
        /// <param name="topology">The geometry topology</param>
        /// <param name="rasterMode">The rasterization mode</param>
        /// <param name="discardRaster">Whether to discard raster results or not</param>
        /// <returns>The builder</returns>
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

        /// <summary>
        /// Creates a new render pipeline builder.
        /// </summary>
        /// <typeparam name="TVertex">The vertex format</typeparam>
        /// <typeparam name="TInstance">The instance format</typeparam>
        /// <param name="vertexShader">The vertex shader</param>
        /// <param name="fragmentShader">The fragment shader</param>
        /// <param name="renderStage">The render stage</param>
        /// <param name="topology">The geometry topology</param>
        /// <param name="rasterMode">The rasterization mode</param>
        /// <param name="discardRaster">Whether to discard raster results or not</param>
        /// <returns>The builder</returns>
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

        /// <summary>
        /// Creates a new vertex buffer builder.
        /// </summary>
        /// <typeparam name="TVertex">The vertex format</typeparam>
        /// <param name="initialData">The initial data</param>
        /// <returns>The builder</returns>
        public VertexBufferBuilder<TVertex> CreateVertexBuffer<TVertex>(
            INativeBuffer<TVertex> initialData
        ) where TVertex : unmanaged
            => new(this, initialData);

        /// <summary>
        /// Creates a new vertex buffer builder.
        /// </summary>
        /// <typeparam name="TVertex">The vertex format</typeparam>
        /// <param name="initialData">The initial data</param>
        /// <returns>The builder</returns>
        public VertexBuffer<TVertex> CreateVertexBuffer<TVertex>(
            params TVertex[] initialData
        ) where TVertex : unmanaged
        {
            using var buf = new NativeBuffer<TVertex>((uint) initialData.Length) {initialData};
            return new VertexBufferBuilder<TVertex>(this, buf);
        }

        /// <summary>
        /// Creates a new vertex buffer builder with a writer.
        /// </summary>
        /// <typeparam name="TVertex">The vertex format</typeparam>
        /// <param name="writer">The writer</param>
        /// <param name="initialData">The inital data</param>
        /// <returns>The builder</returns>
        public VertexBufferBuilder<TVertex> CreateVertexBuffer<TVertex>(
            out VertexBufferWriter<TVertex> writer,
            INativeBuffer<TVertex>? initialData = null
        ) where TVertex : unmanaged
            => new(this, initialData, out writer);

        /// <summary>
        /// Creates a uniform binding builder.
        /// </summary>
        /// <typeparam name="TUniform">The uniform type</typeparam>
        /// <param name="uniform">The uniform handle</param>
        /// <param name="buffer">The uniform buffer</param>
        /// <returns>The builder</returns>
        public UniformBindingBuilder<TUniform> CreateUniformBinding<TUniform>(
            UniformHandle<TUniform> uniform,
            UniformBuffer<TUniform>? buffer = null
        ) where TUniform : unmanaged, IUniform<TUniform>
            => new(this, uniform, buffer);

        /// <summary>
        /// Creates a new uniform buffer.
        /// </summary>
        /// <typeparam name="TUniform">The uniform type</typeparam>
        /// <returns>The buffer</returns>
        public UniformBuffer<TUniform> CreateUniformBuffer<TUniform>(
        ) where TUniform : unmanaged, IUniform<TUniform>
            => new UniformBufferBuilder<TUniform>(this, null);
        
        /// <summary>
        /// Creates a new uniform buffer.
        /// </summary>
        /// <typeparam name="TUniform">The uniform type</typeparam>
        /// <param name="initialData">The initial data</param>
        /// <returns>The buffer</returns>
        public UniformBuffer<TUniform> CreateUniformBuffer<TUniform>(
            INativeBuffer<TUniform> initialData
        ) where TUniform : unmanaged, IUniform<TUniform>
            => new UniformBufferBuilder<TUniform>(this, initialData);

        /// <summary>
        /// Creates a new texture sampler builder.
        /// </summary>
        /// <param name="minFiltering">The minimization filtering</param>
        /// <param name="maxFiltering">The maximization filtering</param>
        /// <param name="wrapping">The wrapping mode</param>
        /// <param name="borderColor">The border color</param>
        /// <returns>The builder</returns>
        public TextureSamplerBuilder CreateTextureSampler(
            TextureFiltering minFiltering = TextureFiltering.Linear,
            TextureFiltering maxFiltering = TextureFiltering.Linear,
            TextureWrapping wrapping = TextureWrapping.Repeat,
            TextureBorderColor borderColor = TextureBorderColor.OpaqueBlack
        ) => new(this, minFiltering, maxFiltering, wrapping, borderColor);
        
        /// <summary>
        /// Creates a new texture binding builder.
        /// </summary>
        /// <param name="shaderSampler">The shader sampler handle</param>
        /// <returns>The builder</returns>
        public TextureBindingBuilder CreateTextureBinding(
            ShaderSamplerHandle shaderSampler
        ) => new(this, shaderSampler, null, null);
        /// <summary>
        /// Creates a new populated texture binding builder.
        /// </summary>
        /// <param name="shaderSampler">The shader sampler handle</param>
        /// <param name="sampler">The sampler</param>
        /// <param name="texture">The texture</param>
        /// <returns>The builder</returns>
        public TextureBindingBuilder CreateTextureBinding(
            ShaderSamplerHandle shaderSampler,
            TextureSampler sampler,
            Texture texture
        ) => new(this, shaderSampler, sampler, texture);

        /// <summary>
        /// Creates a new texture.
        /// </summary>
        /// <param name="image">The image</param>
        /// <returns>The texture</returns>
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

        /// <summary>
        /// Creates a new command buffer builder.
        /// </summary>
        /// <returns>The builder</returns>
        public CommandBufferBuilder CreateCommandBuffer(
        ) => new(this);

        /// <summary>
        /// Enqueues a command buffer for rendering to a target.
        /// </summary>
        /// <param name="target">The target</param>
        /// <param name="cmd">The command buffer</param>
        public void Enqueue(
            IRenderTarget target,
            CommandBuffer cmd
        ) => Bindings.Enqueue(Ptr, target.Handle, cmd.Handle);
    }
}