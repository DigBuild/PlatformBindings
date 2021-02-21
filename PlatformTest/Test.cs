using System;
using DigBuildPlatformCS;
using DigBuildPlatformCS.Resource;
using DigBuildPlatformCS.Util;
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

    public interface IVertexUniforms : IUniform<IVertexUniforms>
    {
        [Uniform(0)]
        public Matrix4x4 Matrix { set; }

        [Uniform(1)]
        public Matrix4x4 Matrix2 { set; }
    }

    public class RenderResources
    {
        public readonly CommandBuffer CommandBuffer;
        // public readonly UniformBuffer<IVertexUniforms> UniformBuffer;
        // public readonly IVertexUniforms Uniforms;
        
        public RenderResources(
            RenderSurfaceContext surface, RenderContext context,
            NativeBufferPool bufferPool, ResourceManager resourceManager)
        {
            VertexShader<IVertexUniforms> vs = context.CreateVertexShader<IVertexUniforms>(
                resourceManager.GetResource(new ResourceName("test", "shaders/test.vert.spv"))!
            );
            FragmentShader fs = context.CreateFragmentShader(
                resourceManager.GetResource(new ResourceName("test", "shaders/test.frag.spv"))!
            );
            RenderPipeline<Vertex> pipeline = context
                .CreatePipeline<Vertex>(surface.RenderStage, Topology.Triangles)
                .WithShader(vs, out var uniformHandle)
                .WithShader(fs);
            
            using var vertexData = bufferPool.Request<Vertex>();
            vertexData.Add(
                new Vertex(0.0f, -0.5f, 0, 1, 0, 0, 1),
                new Vertex(0.5f, 0.5f, 0, 0, 1, 0, 1),
                new Vertex(-0.5f, 0.5f, 0, 0, 0, 1, 1)
            );
            VertexBuffer<Vertex> vb = context.CreateVertexBuffer(vertexData);
            // UniformBuffer = context.CreateUniformBuffer(vs);

            CommandBuffer = context.CreateDrawCommand(out var cbw);
            var cmd = cbw.BeginRecording(surface.Format, bufferPool);
            cmd.SetViewportAndScissor(surface);

            // Uniforms = UniformBuffer.Push();
            // Uniforms.Matrix = Matrix4x4.Identity;
            // cmd.Using(uniformHandle, UniformBuffer, Uniforms);
            
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
            _resources ??= new RenderResources(surface, context, BufferPool, ResourceManager);

            // var milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            // var angle = (milliseconds % 5000) / 5000f;
            // _resources.Uniforms.Matrix = Matrix4x4.CreateRotationZ(angle * 2 * MathF.PI);
            // _resources.UniformBuffer.Upload();

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