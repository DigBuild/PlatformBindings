using System.Numerics;
using System.Threading.Tasks;
using DigBuildPlatformCS;

namespace DigBuildPlatformTest
{
    public interface IUniform
    {
        [Uniform]
        public Matrix4x4 ProjectionMatrix { get; set; }
        [Uniform]
        public Matrix4x4 ViewMatrix { get; set; }
        [Uniform]
        public Matrix4x4 ModelMatrix { get; set; }
    }

    public static class Test
    {
        private static Framebuffer? _myFramebuffer;
        private static FramebufferAttachment _myColorAttachment;

        private static VertexShader<IUniform>? _myShader;

        private static RenderPipeline? _myPipeline;
        private static IUniform _myUniforms = null!;

        private static void Update(RenderSurfaceContext surface, RenderContext context)
        {
            // Framebuffer from surface and updates
            var framebuffer = surface.Framebuffer;
            framebuffer.SetDrawCommands(null!);
            framebuffer.Update();

            // Framebuffer definition and attachment management
            _myFramebuffer ??= context
                .CreateFramebuffer(400, 300)
                .WithColorAttachment(default, out _myColorAttachment);
            var fb2 = context.Get(_myFramebuffer);
            var texture = fb2.GetTexture(_myColorAttachment);

            // Shader definition
            _myShader ??= context.CreateVertexShader<IUniform>();

            // Pipeline definition and uniform manipulation
            _myPipeline ??= context
                .CreatePipeline()
                .With(_myShader, out _myUniforms);
            _myUniforms.ModelMatrix = Matrix4x4.Identity;
        }

        public static async Task Main()
        {
            var surface = await Platform.RequestRenderSurface(Update);
            await surface.Closed;
        }
    }
}