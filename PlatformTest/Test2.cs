using DigBuildPlatformCS;
using DigBuildPlatformCS.Render;
using DigBuildPlatformCS.Resource;
using DigBuildPlatformCS.Util;
using DigBuildPlatformTest.GeneratedUniforms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using DigBuildPlatformCS.Input;

namespace DigBuildPlatformTest
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 Color;

        public Vertex(Vector3 position, Vector3 normal, Vector4 color)
        {
            Position = position;
            Normal = normal;
            Color = color;
        }
    }

    public struct Vertex2
    {
        public Vector2 Position;

        public Vertex2(float x, float y)
        {
            Position = new Vector2(x, y);
        }
    }

    public interface IVertexUniform : IUniform<VertexUniform>
    {
        public Matrix4x4 Matrix { get; set; }
    }

    public class RenderResources
    {
        public readonly Framebuffer Framebuffer;

        public readonly UniformHandle<VertexUniform> Uniform;
        public readonly UniformBuffer<VertexUniform> UniformBuffer;
        public readonly PooledNativeBuffer<VertexUniform> UniformNativeBuffer;

        public readonly VertexBufferWriter<Vertex> MainVertexBuffer;
        public readonly PooledNativeBuffer<Vertex> MainVertexNativeBuffer;

        public readonly CommandBuffer MainCommandBuffer;
        public readonly CommandBuffer CompCommandBuffer;

        public RenderResources(
            RenderSurfaceContext surface, RenderContext context,
            NativeBufferPool bufferPool, ResourceManager resourceManager
        )
        {
            // Custom framebuffer format and render stages for preliminary rendering
            FramebufferFormat framebufferFormat = context
                .CreateFramebufferFormat()
                .WithDepthStencilAttachment(out var depthStencilAttachment)
                .WithColorAttachment(out var colorAttachment, TextureFormat.R8G8B8A8SRGB, new Vector4(0, 0, 0, 1))
                .WithStage(out var mainRenderStage, depthStencilAttachment, colorAttachment);

            // Framebuffer for preliminary rendering
            Framebuffer = context.CreateFramebuffer(framebufferFormat, surface.Width, surface.Height);

            IResource vsMainResource = resourceManager.GetResource(new ResourceName("test", "shaders/test2.vert.spv"))!;
            IResource fsMainResource = resourceManager.GetResource(new ResourceName("test", "shaders/test2.frag.spv"))!;
            IResource vsCompResource = resourceManager.GetResource(new ResourceName("test", "shaders/comp.vert.spv"))!;
            IResource fsCompResource = resourceManager.GetResource(new ResourceName("test", "shaders/comp.frag.spv"))!;

            // Main geometry pipeline
            VertexShader vsMain = context
                .CreateVertexShader(vsMainResource)
                .WithUniform(out Uniform);
            FragmentShader fsMain = context.CreateFragmentShader(fsMainResource);
            RenderPipeline<Vertex> mainPipeline = context.CreatePipeline<Vertex>(
                vsMain, fsMain,
                mainRenderStage,
                Topology.Triangles,
                depthTest: new DepthTest(true, CompareOperation.LessOrEqual, true)
            ).WithStandardBlending(colorAttachment);

            // Secondary geometry pipeline for compositing
            VertexShader vsComp = context.CreateVertexShader(vsCompResource);
            FragmentShader fsComp = context
                .CreateFragmentShader(fsCompResource)
                .WithSampler(out var colorTextureHandle);
            RenderPipeline<Vertex2> compPipeline = context.CreatePipeline<Vertex2>(
                vsComp, fsComp,
                surface.RenderStage,
                Topology.Triangles
            ).WithStandardBlending(surface.ColorAttachment);

            // Main vertex buffer w/ external writer
            VertexBuffer<Vertex> mainVertexBuffer = context.CreateVertexBuffer(out MainVertexBuffer);
            MainVertexNativeBuffer = bufferPool.Request<Vertex>();

            UniformNativeBuffer = bufferPool.Request<VertexUniform>();
            UniformNativeBuffer.Add(
                new VertexUniform{
                    Matrix = Matrix4x4.Identity
                }
            );
            UniformBuffer = context.CreateUniformBuffer(Uniform, UniformNativeBuffer);

            // Composition vertex buffer, pre-filled with screen rectangle
            using var compVertexData = bufferPool.Request<Vertex2>();
            compVertexData.Add(
                // Tri 1
                new Vertex2(0, 0),
                new Vertex2(1, 0),
                new Vertex2(1, 1),
                // Tri 2
                new Vertex2(1, 1),
                new Vertex2(0, 1),
                new Vertex2(0, 0)
            );
            VertexBuffer<Vertex2> compVertexBuffer = context.CreateVertexBuffer(compVertexData);

            // Create sampler and texture binding
            TextureSampler sampler = context.CreateTextureSampler();
            TextureBinding fbTextureBinding = context.CreateTextureBinding(
                colorTextureHandle,
                sampler,
                Framebuffer.Get(colorAttachment)
            );

            // Create overlay texture and binding
            IResource overlayTextureResource = resourceManager.GetResource(new ResourceName("test", "textures/overlay_thing.png"))!;
            Texture overlayTexture = context.CreateTexture(new Bitmap(overlayTextureResource.OpenStream()));
            TextureBinding overlayTextureBinding = context.CreateTextureBinding(
                colorTextureHandle,
                sampler,
                overlayTexture
            );

            // Record commandBuffers
            MainCommandBuffer = context.CreateCommandBuffer();
            var mcmd = MainCommandBuffer.BeginRecording(framebufferFormat, bufferPool);
            mcmd.SetViewportAndScissor(Framebuffer);
            mcmd.Using(mainPipeline, UniformBuffer, 0);
            mcmd.Draw(mainPipeline, mainVertexBuffer);
            mcmd.Commit(context);

            CompCommandBuffer = context.CreateCommandBuffer();
            var ccmd = CompCommandBuffer.BeginRecording(surface.Format, bufferPool);
            ccmd.SetViewportAndScissor(surface);
            ccmd.Using(compPipeline, fbTextureBinding); // Framebuffer
            ccmd.Draw(compPipeline, compVertexBuffer);
            ccmd.Using(compPipeline, overlayTextureBinding); // Overlay
            ccmd.Draw(compPipeline, compVertexBuffer);
            ccmd.Commit(context);
        }
    }

    public static class Test
    {
        private static readonly NativeBufferPool BufferPool = new();
        private static readonly ResourceManager ResourceManager = new(
            new FileSystemResourceProvider(
                new Dictionary<string, string>
                {
                    ["test"] = "../../PlatformTest/Resources"
                }
            )
        );
        private static RenderResources? _resources;
        private static Controller? _controller;

        private static void Update(RenderSurfaceContext surface, RenderContext context)
        {
            // Create render resources if not already available
            if (_resources == null)
            {
                _resources = new RenderResources(surface, context, BufferPool, ResourceManager);
                
                var vnb = _resources.MainVertexNativeBuffer;
                vnb.Clear();
                vnb.Add(CreateCubeGeometry(new Vector3(-1.1f, -0.5f, -0.5f), new Vector4(0, 0, 1, 0.5f), BlockFaceFlags.All).ToArray());
                vnb.Add(CreateCubeGeometry(new Vector3(0.5f, -0.5f, -0.5f), new Vector4(1, 0, 0, 0.5f), BlockFaceFlags.All).ToArray());
                _resources.MainVertexBuffer.Write(vnb);
            }
            
            // Calculate the new angle
            var milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var angle = (milliseconds % 5000) / 5000f;
            
            // Update the uniform transformation matrix on the GPU
            var unb = _resources.UniformNativeBuffer;
            unb[0].Matrix = Matrix4x4.CreateRotationY(angle * 2 * MathF.PI)
                            * Matrix4x4.CreateTranslation(0, -1f, -3f)
                            * Matrix4x4.CreateScale(1, -1, 1)
                            * Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f,
                                surface.Width / (float) surface.Height, 0.01f, 1000f);
            _resources.UniformBuffer.Write(unb);

            context.Enqueue(_resources.Framebuffer, _resources.MainCommandBuffer);
            context.Enqueue(surface, _resources.CompCommandBuffer);
        }

        public static async Task Main()
        {
            var surface = await Platform.RequestRenderSurface(
                Update,
                widthHint: 800,
                heightHint: 600,
                titleHint: "Platform Bindings Test"
            );
            await surface.Closed;
        }


        [Flags]
        internal enum BlockFaceFlags
        {
            None = 0,
            NegX = 1 << 0,
            PosX = 1 << 1,
            NegY = 1 << 2,
            PosY = 1 << 3,
            NegZ = 1 << 4,
            PosZ = 1 << 5,
            All = NegX | NegY | PosX | PosY | NegZ | PosZ
        }

        private static IEnumerable<Vertex> CreateCubeGeometry(Vector3 offset, Vector4 color, BlockFaceFlags faces)
        {
            var nx = Vector3.Zero;
            var ny = Vector3.Zero;
            var nz = Vector3.Zero;
            var px = Vector3.UnitX;
            var py = Vector3.UnitY;
            var pz = Vector3.UnitZ;

            if (faces.HasFlag(BlockFaceFlags.NegX))
            {
                // Negative X
                yield return new Vertex(offset + nx + ny + nz, -px, color);
                yield return new Vertex(offset + nx + py + nz, -px, color);
                yield return new Vertex(offset + nx + py + pz, -px, color);

                yield return new Vertex(offset + nx + ny + pz, -px, color);
                yield return new Vertex(offset + nx + ny + nz, -px, color);
                yield return new Vertex(offset + nx + py + pz, -px, color);
            }
            if (faces.HasFlag(BlockFaceFlags.PosX))
            {
                // Positive X
                yield return new Vertex(offset + px + ny + nz, px, color);
                yield return new Vertex(offset + px + py + pz, px, color);
                yield return new Vertex(offset + px + py + nz, px, color);

                yield return new Vertex(offset + px + py + pz, px, color);
                yield return new Vertex(offset + px + ny + nz, px, color);
                yield return new Vertex(offset + px + ny + pz, px, color);
            }

            if (faces.HasFlag(BlockFaceFlags.NegY))
            {
                // Negative Y
                yield return new Vertex(offset + nx + ny + nz, -py, color);
                yield return new Vertex(offset + px + ny + pz, -py, color);
                yield return new Vertex(offset + px + ny + nz, -py, color);

                yield return new Vertex(offset + px + ny + pz, -py, color);
                yield return new Vertex(offset + nx + ny + nz, -py, color);
                yield return new Vertex(offset + nx + ny + pz, -py, color);
            }

            if (faces.HasFlag(BlockFaceFlags.PosY))
            {
                // Positive Y
                yield return new Vertex(offset + nx + py + nz, py, color);
                yield return new Vertex(offset + px + py + nz, py, color);
                yield return new Vertex(offset + px + py + pz, py, color);

                yield return new Vertex(offset + px + py + pz, py, color);
                yield return new Vertex(offset + nx + py + pz, py, color);
                yield return new Vertex(offset + nx + py + nz, py, color);
            }

            if (faces.HasFlag(BlockFaceFlags.NegZ))
            {
                // Negative Z
                yield return new Vertex(offset + nx + ny + nz, -pz, color);
                yield return new Vertex(offset + px + ny + nz, -pz, color);
                yield return new Vertex(offset + px + py + nz, -pz, color);

                yield return new Vertex(offset + px + py + nz, -pz, color);
                yield return new Vertex(offset + nx + py + nz, -pz, color);
                yield return new Vertex(offset + nx + ny + nz, -pz, color);
            }

            if (faces.HasFlag(BlockFaceFlags.PosZ))
            {
                // Positive Z
                yield return new Vertex(offset + px + py + pz, pz, color);
                yield return new Vertex(offset + px + ny + pz, pz, color);
                yield return new Vertex(offset + nx + ny + pz, pz, color);

                yield return new Vertex(offset + nx + ny + pz, pz, color);
                yield return new Vertex(offset + nx + py + pz, pz, color);
                yield return new Vertex(offset + px + py + pz, pz, color);
            }
        }
    }
}