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

    public struct Instance
    {
        public Vector3 Offset;
        public float Size;

        public Instance(float x, float y, float z, float size)
        {
            Offset = new Vector3(x, y, z);
            Size = size;
        }
    }

    public interface IVertexUniform : IUniform<VertexUniform>
    {
        public Matrix4x4 Matrix { set; }
    }
    
    public class RenderResources
    {
        public readonly CommandBuffer CommandBuffer;

        public readonly VertexBufferWriter<Instance> InstanceBuffer;
        public readonly PooledNativeBuffer<Instance> InstanceNativeBuffer;

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
            RenderPipeline<Vertex, Instance> pipeline = context.CreatePipeline<Vertex, Instance>(
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

            InstanceNativeBuffer = bufferPool.Request<Instance>();
            InstanceNativeBuffer.Add(
                new Instance(-0.25f, 0, 0, 1),
                new Instance(0.25f, 0.25f, 0, 1)
            );
            VertexBuffer<Instance> ib = context.CreateVertexBuffer(
                out InstanceBuffer,
                InstanceNativeBuffer
            );

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
            cmd.Draw(pipeline, vb, ib);
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

            // Update the uniform transformation matrix on the GPU
            var unb = _resources.UniformNativeBuffer;
            unb[0].Matrix = Matrix4x4.CreateRotationZ(angle * 2 * MathF.PI)
                            * Matrix4x4.CreateScale(1, 800 / 600f, 0);
            _resources.UniformBuffer.Write(unb);

            // Update the instance size on the GPU
            var inb = _resources.InstanceNativeBuffer;
            inb[0].Size = MathF.Cos(angle * 4 * MathF.PI);
            inb[1].Size = MathF.Cos(angle * 6 * MathF.PI);
            _resources.InstanceBuffer.Write(inb);

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