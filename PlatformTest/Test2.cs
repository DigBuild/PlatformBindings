// using System.Collections.Generic;
// using System.Numerics;
// using System.Threading.Tasks;
// using DigBuildPlatformCS;
// using DigBuildPlatformCS.Util;
//
// namespace DigBuildPlatformTest2
// {
//     public struct Vertex
//     {
//         public Vector3 Position;
//         public Vector3 Normal;
//         public Vector4 Color;
//     }
//
//     public struct Instance
//     {
//         public Vector3 Offset;
//
//         public Instance(float x, float y, float z)
//         {
//             Offset = new Vector3(x, y, z);
//         }
//     }
//
//     public struct Vertex2
//     {
//         public Vector2 Position;
//
//         public Vertex2(float x, float y)
//         {
//             Position = new Vector2(x, y);
//         }
//     }
//
//     public interface IVertexUniforms
//     {
//         [Uniform(0)]
//         public Matrix4x4 ProjectionMatrix { set; }
//         [Uniform(1)]
//         public Matrix4x4 ModelViewMatrix { set; }
//     }
//
//     public interface IFragmentUniforms
//     {
//         [Uniform(0)]
//         public Texture ColorTexture { set; }
//         [Uniform(1)]
//         public Texture BloomTexture { set; }
//         [Uniform(2)]
//         public Texture DepthTexture { set; }
//     }
//
//     public class RenderResources
//     {
//         public readonly IVertexUniforms VertexUniforms1, VertexUniforms2;
//         public readonly VertexBufferWriter<Vertex> MainVertexBuffer;
//         public readonly VertexBufferWriter<Instance> BloomInstanceBuffer;
//         public readonly CommandBuffer CommandBuffer;
//
//         public RenderResources(RenderSurfaceContext surface, RenderContext context, NativeBufferPool bufferPool)
//         {
//             // Custom framebuffer format and render stages for preliminary rendering
//             FramebufferFormat framebufferFormat = context
//                 .CreateFramebufferFormat()
//                 .WithColorAttachment(out var colorAttachment, TextureFormat.RGBA8, Vector4.Zero)
//                 .WithColorAttachment(out var bloomAttachment, TextureFormat.RGBA8, Vector4.Zero)
//                 .WithDepthStencilAttachment(out var depthStencilAttachment)
//                 .WithStage(out var mainRenderStage, colorAttachment, depthStencilAttachment)
//                 .WithStage(out var bloomRenderStage, colorAttachment, bloomAttachment, depthStencilAttachment)
//                 .WithDependency(bloomRenderStage, mainRenderStage);
//
//             // Framebuffer for preliminary rendering
//             Framebuffer framebuffer = context.CreateFramebuffer(framebufferFormat, surface.Framebuffer.Width, surface.Framebuffer.Height);
//
//             // Main geometry pipeline
//             VertexShader<IVertexUniforms> vsMain = context.CreateVertexShader<IVertexUniforms>();
//             FragmentShader fsMain = context.CreateFragmentShader();
//             RenderPipeline<Vertex> mainPipeline = context
//                 .CreatePipeline<Vertex>(mainRenderStage, Topology.Triangles)
//                 .WithShader(vsMain, out VertexUniforms1)
//                 .WithShader(fsMain)
//                 .WithStandardBlending(0);
//
//             // Secondary geometry pipeline for bloom
//             VertexShader<IVertexUniforms> vsBloom = context.CreateVertexShader<IVertexUniforms>();
//             FragmentShader fsBloom = context.CreateFragmentShader();
//             RenderPipeline<Vertex, Instance> bloomPipeline = context
//                 .CreatePipeline<Vertex, Instance>(bloomRenderStage, Topology.Triangles)
//                 .WithShader(vsBloom, out VertexUniforms2)
//                 .WithShader(fsBloom)
//                 .WithStandardBlending(0);
//
//             // Composition pipeline
//             VertexShader vsComp = context.CreateVertexShader();
//             FragmentShader<IFragmentUniforms> fsComp = context.CreateFragmentShader<IFragmentUniforms>();
//             RenderPipeline<Vertex2> compositionPipeline = context
//                 .CreatePipeline<Vertex2>(surface.RenderStage, Topology.TriangleStrips)
//                 .WithShader(vsComp)
//                 .WithShader(fsComp, out var myFragmentUniforms)
//                 .WithStandardBlending(0);
//             // Set fragment uniforms to the textures from the first pass
//             myFragmentUniforms.ColorTexture = framebuffer.Get(colorAttachment);
//             myFragmentUniforms.BloomTexture = framebuffer.Get(bloomAttachment);
//             myFragmentUniforms.DepthTexture = framebuffer.Get(depthStencilAttachment);
//
//             // Main vertex buffer w/ external writer
//             VertexBuffer<Vertex> mainVertexBuffer = context.CreateVertexBuffer(out MainVertexBuffer);
//
//             // Secondary vertex buffer, pre-filled, and secondary index buffer w/ external writer
//             using var bloomVertexData = bufferPool.Request<Vertex>();
//             FillBloomVertexData(bloomVertexData);
//             VertexBuffer<Vertex> bloomVertexBuffer = context.CreateVertexBuffer(bloomVertexData);
//             VertexBuffer<Instance> bloomInstanceBuffer = context.CreateVertexBuffer(out BloomInstanceBuffer);
//
//             // Composition vertex buffer, pre-filled with screen rectangle
//             using var compVertexData = bufferPool.Request<Vertex2>();
//             compVertexData.Add(
//                 new Vertex2(0, 0),
//                 new Vertex2(1, 0),
//                 new Vertex2(1, 1),
//                 new Vertex2(0, 1)
//             );
//             VertexBuffer<Vertex2> compVertexBuffer = context.CreateVertexBuffer(compVertexData);
//
//             // Draw commandBuffer for the entire render process
//             CommandBuffer = context.CreateDrawCommand();
//             using (var cmd = CommandBuffer.BeginRecording())
//             {
//                 cmd.SetAndClearRenderTarget(
//                     framebuffer,
//                     new Dictionary<FramebufferAttachment, Vector4>()
//                     {
//                         [colorAttachment] = Vector4.One
//                     }
//                 );
//                 cmd.Draw(mainPipeline, mainVertexBuffer);
//                 cmd.Draw(bloomPipeline, bloomVertexBuffer, bloomInstanceBuffer);
//
//                 cmd.SetAndClearRenderTarget(surface.Framebuffer);
//                 cmd.Draw(compositionPipeline, compVertexBuffer);
//             }
//         }
//
//         private static void FillBloomVertexData(PooledNativeBuffer<Vertex> bloomVertexData)
//         {
//             throw new System.NotImplementedException();
//         }
//     }
//
//     public static class Test
//     {
//         private static readonly NativeBufferPool BufferPool = new();
//         private static RenderResources? _resources;
//
//         private static void Update(RenderSurfaceContext surface, RenderContext context)
//         {
//             // Create render resources if not already available and initialize projection matrices
//             if (_resources == null)
//             {
//                 _resources = new RenderResources(surface, context, BufferPool);
//                 _resources.VertexUniforms1.ProjectionMatrix = Matrix4x4.Identity;
//                 _resources.VertexUniforms2.ProjectionMatrix = Matrix4x4.Identity;
//             }
//
//             // Set main stage transforms and write geometry
//             _resources.VertexUniforms1.ModelViewMatrix = Matrix4x4.Identity;
//             using var mainGeometry = BufferPool.Request<Vertex>();
//             // mainGeometry.Add(
//             //     
//             // );
//             _resources.MainVertexBuffer.Write(mainGeometry);
//
//             // Set bloom stage transforms and write geometry
//             _resources.VertexUniforms2.ModelViewMatrix = Matrix4x4.Identity;
//             using var bloomInstances = BufferPool.Request<Instance>();
//             bloomInstances.Add(
//                 new Instance(1, 0, 0)
//             );
//             _resources.BloomInstanceBuffer.Write(bloomInstances);
//
//             // Enqueue draw commandBuffer
//             context.Enqueue(_resources.CommandBuffer);
//         }
//
//         public static async Task Main()
//         {
//             var surface = await Platform.RequestRenderSurface(
//                 Update,
//                 widthHint: 800,
//                 heightHint: 600,
//                 titleHint: "Platform Bindings Test"
//             );
//             await surface.Closed;
//         }
//     }
// }