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

    // public interface IVertexUniforms : IUniform<IVertexUniforms>
    // {
    //     [Uniform(0)]
    //     public Matrix4x4 Matrix { set; }
    // }

    public class RenderResources
    {
        // public readonly UniformHandle<IVertexUniforms> Uniforms;
        public readonly VertexBufferWriter<Vertex> VertexBufferWriter;
        public readonly CommandBuffer CommandBuffer;
        
        public RenderResources(
            RenderSurfaceContext surface, RenderContext context,
            NativeBufferPool bufferPool, ResourceManager resourceManager)
        {
            VertexShader/* <IVertexUniforms> */ vs = context.CreateVertexShader/* <IVertexUniforms> */(
                resourceManager.GetResource(new ResourceName("test", "shaders/test.vert.spv"))!
            );
            FragmentShader fs = context.CreateFragmentShader(
                resourceManager.GetResource(new ResourceName("test", "shaders/test.frag.spv"))!
            );
            RenderPipeline<Vertex> pipeline = context
                .CreatePipeline<Vertex>(surface.RenderStage, Topology.Triangles)
                .WithShader(vs/* , out Uniforms */)
                .WithShader(fs);
            
            VertexBuffer<Vertex> vb = context.CreateVertexBuffer(out VertexBufferWriter);

            CommandBuffer = context.CreateDrawCommand(out var cbw);
            var cmd = cbw.BeginRecording(surface.Format, bufferPool);
            cmd.SetViewportAndScissor(surface);
            // var uniforms = cmd.Push(Uniforms);
            // uniforms.Matrix = Matrix4x4.Identity;
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
            if (_resources == null)
            {
                _resources = new RenderResources(surface, context, BufferPool, ResourceManager);
            //     _resources.Uniforms.Matrix = Matrix4x4.Identity;
            }

            var milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var angle = (milliseconds % 5000) / 5000f;

            var sc = 0.5f;
            var si = MathF.Sin(2 * MathF.PI / 3f) * sc;
            var co = MathF.Sin(2 * MathF.PI / 3f) * sc;

            var mat = Matrix4x4.CreateRotationZ(angle * 2 * MathF.PI);
            var a = Vector3.Transform(new Vector3(0.0f, -sc, 0), mat);
            var b = Vector3.Transform(new Vector3(co, si, 0), mat);
            var c = Vector3.Transform(new Vector3(-co, si, 0), mat);

            using var vertexData = BufferPool.Request<Vertex>();
            vertexData.Add(
                new Vertex(a.X, a.Y, a.Z, 1, 0, 0, 1),
                new Vertex(b.X, b.Y, b.Z, 0, 1, 0, 1),
                new Vertex(c.X, c.Y, c.Z, 0, 0, 1, 1)
            );
            _resources.VertexBufferWriter.Write(vertexData);

            context.Enqueue(surface, _resources.CommandBuffer);
        }

        public static async Task Main()
        {
            var surface = await Platform.RequestRenderSurface(
                Update,
                widthHint: 600,
                heightHint: 600,
                titleHint: "Platform Bindings Test"
            );
            await surface.Closed;
        }
    }
}