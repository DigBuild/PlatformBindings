using System.Numerics;
using System.Threading.Tasks;
using DigBuildPlatformCS;

namespace DigBuildPlatformTest
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 Color;
    }

    public struct Instance
    {
        public Vector3 Offset;
    }

    public struct Vertex2
    {
        public Vector2 Position;
    }

    public interface IVertexUniforms
    {
        [Uniform(0)]
        public Matrix4x4 ProjectionMatrix { set; }
        [Uniform(1)]
        public Matrix4x4 ModelViewMatrix { set; }
    }

    public interface IFragmentUniforms
    {
        [Uniform(0)]
        public Texture ColorTexture { set; }
        [Uniform(1)]
        public Texture BloomTexture { set; }
        [Uniform(2)]
        public Texture DepthTexture { set; }
    }

    public class RenderResources
    {
        public readonly IVertexUniforms VertexUniforms1, VertexUniforms2;
        public readonly VertexBufferWriter<Vertex> MainVertexBuffer;
        public readonly VertexBufferWriter<Instance> BloomInstanceBuffer;
        public readonly DrawCommand DrawCommand;

        public RenderResources(RenderSurfaceContext surface, RenderContext context)
        {
            // Custom framebuffer format and render stages for preliminary rendering
            FramebufferFormat framebufferFormat = context
                .CreateFramebufferFormat()
                .WithColorAttachment(out var colorAttachment, default, Vector4.Zero)
                .WithColorAttachment(out var bloomAttachment, default, Vector4.Zero)
                .WithDepthStencilAttachment(out var depthStencilAttachment, default)
                .WithStage(out var mainRenderStage, colorAttachment, depthStencilAttachment)
                .WithStage(out var bloomRenderStage, colorAttachment, bloomAttachment, depthStencilAttachment)
                .WithDependency(bloomRenderStage, mainRenderStage);

            // Framebuffer for preliminary rendering
            var frt = context.Get(surface.Framebuffer);
            Framebuffer framebuffer = context.CreateFramebuffer(framebufferFormat, frt.Width, frt.Height);

            // Main geometry pipeline
            VertexShader<IVertexUniforms> vsMain = context.CreateVertexShader<IVertexUniforms>();
            FragmentShader fsMain = context.CreateFragmentShader();
            RenderPipeline<Vertex> mainPipeline = context
                .CreatePipeline<Vertex>(mainRenderStage, Topology.Triangles)
                .WithShader(vsMain, out VertexUniforms1)
                .WithShader(fsMain)
                .WithStandardBlending(0);

            // Secondary geometry pipeline for bloom
            VertexShader<IVertexUniforms> vsBloom = context.CreateVertexShader<IVertexUniforms>();
            FragmentShader fsBloom = context.CreateFragmentShader();
            RenderPipeline<Vertex, Instance> bloomPipeline = context
                .CreatePipeline<Vertex, Instance>(bloomRenderStage, Topology.Triangles)
                .WithShader(vsBloom, out VertexUniforms2)
                .WithShader(fsBloom)
                .WithStandardBlending(0);

            // Composition pipeline
            VertexShader vsComp = context.CreateVertexShader();
            FragmentShader<IFragmentUniforms> fsComp = context.CreateFragmentShader<IFragmentUniforms>();
            RenderPipeline<Vertex2> compositionPipeline = context
                .CreatePipeline<Vertex2>(surface.RenderStage, Topology.TriangleStrips)
                .WithShader(vsComp)
                .WithShader(fsComp, out var myFragmentUniforms)
                .WithStandardBlending(0);
            // Set fragment uniforms to the textures from the first pass
            var fb = context.Get(framebuffer);
            myFragmentUniforms.ColorTexture = fb.Get(colorAttachment);
            myFragmentUniforms.BloomTexture = fb.Get(bloomAttachment);
            myFragmentUniforms.DepthTexture = fb.Get(depthStencilAttachment);

            // Main vertex buffer w/ external writer
            VertexBuffer<Vertex> mainVertexBuffer = context.CreateVertexBuffer(out MainVertexBuffer);

            // Secondary vertex buffer, pre-filled, and secondary index buffer w/ external writer
            VertexBuffer<Vertex> bloomVertexBuffer = context.CreateVertexBuffer<Vertex>(null!);
            VertexBuffer<Instance> bloomInstanceBuffer = context.CreateVertexBuffer(out BloomInstanceBuffer);

            // Composition vertex buffer, pre-filled with screen rectangle
            VertexBuffer<Vertex2> compVertexBuffer = context.CreateVertexBuffer<Vertex2>(null!);

            // Draw command for the entire render process
            DrawCommand = context
                .CreateDrawCommand()
                .WithRenderTarget(framebuffer)
                .WithClearColor(colorAttachment, Vector4.One)
                .With(mainPipeline, mainVertexBuffer)
                .With(bloomPipeline, bloomVertexBuffer, bloomInstanceBuffer)
                .WithRenderTarget(surface.Framebuffer)
                .With(compositionPipeline, compVertexBuffer);
        }
    }

    public static class Test
    {
        private static RenderResources? _resources;

        private static void Update(RenderSurfaceContext surface, RenderContext context)
        {
            // Create render resources if not already available and initialize projection matrices
            if (_resources == null)
            {
                _resources = new RenderResources(surface, context);
                _resources.VertexUniforms1.ProjectionMatrix = Matrix4x4.Identity;
                _resources.VertexUniforms2.ProjectionMatrix = Matrix4x4.Identity;
            }
            
            // Set main stage transforms and write geometry
            _resources.VertexUniforms1.ModelViewMatrix = Matrix4x4.Identity;
            var vbMain = context.Get(_resources.MainVertexBuffer);
            vbMain.Write(null!);

            // Set bloom stage transforms and write geometry
            _resources.VertexUniforms2.ModelViewMatrix = Matrix4x4.Identity;
            var vbBloom = context.Get(_resources.BloomInstanceBuffer);
            vbBloom.Write(null!);

            // Enqueue draw command
            context.Enqueue(_resources.DrawCommand);
        }

        public static async Task Main()
        {
            var surface = await Platform.RequestRenderSurface(Update, 800, 600, "Platform Bindings Test");
            await surface.Closed;
        }
    }
}