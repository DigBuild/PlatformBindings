using DigBuildPlatformCS;
using DigBuildPlatformCS.Resource;
using DigBuildPlatformCS.Util;
using DigBuildPlatformTest.GeneratedUniforms;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace DigBuildPlatformTest
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector4 Color;

        public Vertex(float x, float y, float z, float r, float g, float b, float a)
        {
            Position = new Vector3(x, y, z);
            Color = new Vector4(r, g, b, a);
        }
    }

    public interface IVertexUniform : IUniform<VertexUniform>
    {
        public Matrix4x4 Matrix { set; }
    }
    
    public class RenderResources
    {
        public readonly CommandBuffer CommandBuffer;

        public readonly UniformHandle<VertexUniform> Uniform;
        public readonly UniformBuffer<VertexUniform> UniformBuffer;
        public readonly PooledNativeBuffer<VertexUniform> UniformNativeBuffer;

        public RenderResources(
            RenderSurfaceContext surface, RenderContext context,
            NativeBufferPool bufferPool, ResourceManager resourceManager)
        {
            IResource vsResource = resourceManager.GetResource(new ResourceName("test", "shaders/test.vert.spv"))!;
            IResource fsResource = resourceManager.GetResource(new ResourceName("test", "shaders/test.frag.spv"))!;

            VertexShader vs = context.CreateVertexShader(vsResource)
                .WithUniform(out Uniform);
            FragmentShader fs = context.CreateFragmentShader(fsResource);
            RenderPipeline<Vertex> pipeline = context.CreatePipeline<Vertex>(
                vs, fs,
                surface.RenderStage,
                Topology.Triangles
            );

            float s = MathF.Cos(MathF.PI / 3);
            float c = MathF.Sin(MathF.PI / 3);

            using var vertexData = bufferPool.Request<Vertex>();
            vertexData.Add(
                new Vertex(0.0f, -0.5f, 0, 1, 0, 0, 1),
                new Vertex(c * 0.5f, s * 0.5f, 0, 0, 1, 0, 1),
                new Vertex(c * -0.5f, s * 0.5f, 0, 0, 0, 1, 1)
            );
            VertexBuffer<Vertex> vb = context.CreateVertexBuffer(vertexData);

            UniformNativeBuffer = bufferPool.Request<VertexUniform>();
            UniformNativeBuffer.Add(
                new VertexUniform{
                    Matrix = Matrix4x4.Identity
                }
            );
            UniformBuffer = context.CreateUniformBuffer(Uniform, UniformNativeBuffer);

            CommandBuffer = context.CreateDrawCommand(out var cbw);
            var cmd = cbw.BeginRecording(surface.Format, bufferPool);
            cmd.SetViewportAndScissor(surface);
            cmd.Using(pipeline, UniformBuffer, 0);
            cmd.Draw(pipeline, vb);
            cmd.Commit(context);
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
        
        private static void Update(RenderSurfaceContext surface, RenderContext context)
        {
            // Allocate resources if we haven't yet
            _resources ??= new RenderResources(surface, context, BufferPool, ResourceManager);

            // Calculate the new angle
            var milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var angle = (milliseconds % 5000) / 5000f;

            // Update the angle on the GPU
            var unb = _resources.UniformNativeBuffer;
            unb[0].Matrix = Matrix4x4.CreateRotationZ(angle * 2 * MathF.PI)
                            * Matrix4x4.CreateScale(1, 800 / 600f, 0);
            _resources.UniformBuffer.Write(unb);

            // Enqueue draw commands
            context.Enqueue(surface, _resources.CommandBuffer);
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
    }
}