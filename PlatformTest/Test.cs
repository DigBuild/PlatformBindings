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

    public interface IVertexUniforms
    {
        [Uniform(0)]
        public Matrix4x4 ProjectionMatrix { get; set; }
        [Uniform(1)]
        public Matrix4x4 ModelViewMatrix { get; set; }
    }

    public interface IFragmentUniforms
    {
        // [Uniform(0)]
        // public TextureSampler Texture { get; set; }
    }

    public static class Test
    {
        private static Framebuffer? _myFramebuffer;
        private static FramebufferAttachment _myColorAttachment, _myDepthStencilAttachment;

        private static VertexShader<IVertexUniforms>? _myVertexShader;
        private static FragmentShader<IFragmentUniforms>? _myFragmentShader;

        private static RenderPipeline<Vertex>? _myPipeline;
        private static IVertexUniforms _myVertexUniforms = null!;
        private static IFragmentUniforms _myFragmentUniforms = null!;

        private static VertexBuffer<Vertex>? _myVertexBuffer;
        private static VertexBufferWriter<Vertex> _myVertexBufferWriter;

        private static DrawCommand? _myDrawCommand;

        private static void Update(RenderSurfaceContext surface, RenderContext context)
        {
            // Framebuffer definition and attachment management
            _myFramebuffer ??= context
                .CreateFramebuffer(400, 300)
                .WithColorAttachment(default, out _myColorAttachment)
                .WithDepthStencilAttachment(default, out _myDepthStencilAttachment);
            var fb = context.Get(_myFramebuffer);
            var texture = fb.Get(_myColorAttachment);

            // Shader definition
            _myVertexShader ??= context.CreateVertexShader<IVertexUniforms>();
            _myFragmentShader ??= context.CreateFragmentShader<IFragmentUniforms>();

            // Pipeline definition and uniform manipulation
            _myPipeline ??= context
                .CreatePipeline<Vertex>(Topology.Triangles)
                .With(_myVertexShader, out _myVertexUniforms)
                .With(_myFragmentShader, out _myFragmentUniforms);
            _myVertexUniforms.ProjectionMatrix = Matrix4x4.Identity;
            _myVertexUniforms.ModelViewMatrix = Matrix4x4.Identity;

            // Vertex buffer creation and writing
            _myVertexBuffer ??= context
                .CreateVertexBuffer<Vertex>()
                .WithWriter(out _myVertexBufferWriter);
            var vb = context.Get(_myVertexBufferWriter);
            vb.Write(null!);

            // Draw command creation
            _myDrawCommand ??= context.CreateDrawCommand(_myPipeline, _myVertexBuffer);
            
            // Framebuffer from surface and update
            var framebuffer = surface.Framebuffer;
            framebuffer.SetDrawCommands(_myDrawCommand);
            framebuffer.Update();
        }

        public static async Task Main()
        {
            var surface = await Platform.RequestRenderSurface(Update);
            await surface.Closed;
        }
    }
}