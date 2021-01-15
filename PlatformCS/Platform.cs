using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AdvancedDLSupport;
using DigBuildPlatformCS.Util;

namespace DigBuildPlatformCS
{
    [NativeSymbols("dbp_platform_", SymbolTransformationMethod.Underscore)]
    internal interface IPlatformBindings
    {
        bool SupportsMultipleRenderSurfaces();

        IntPtr RequestRenderSurface(
            NativeRenderSurfaceUpdateDelegate update,
            RenderSurfaceCreationHints hints
        );
    }

    public static class Platform
    {
        internal static readonly IPlatformBindings Bindings = NativeLib.Get<IPlatformBindings>();

        public static bool SupportsMultipleRenderSurfaces => Bindings.SupportsMultipleRenderSurfaces();

        public static RenderSurfaceRequestBuilder RequestRenderSurface(
            RenderSurface.UpdateDelegate update,
            uint widthHint = 800,
            uint heightHint = 600,
            string titleHint = "",
            bool fullscreenHint = false
        )
        {
            return new(
                update,
                new RenderSurfaceCreationHints
                {
                    Width = widthHint,
                    Height = heightHint,
                    Title = titleHint,
                    Fullscreen = fullscreenHint
                }
            );
        }
    }

    public readonly ref struct RenderSurfaceRequestBuilder
    {
        private readonly RenderSurface.UpdateDelegate _update;
        private readonly RenderSurfaceCreationHints _hints;

        internal RenderSurfaceRequestBuilder(RenderSurface.UpdateDelegate update, RenderSurfaceCreationHints hints)
        {
            _update = update;
            _hints = hints;
        }

        public TaskAwaiter<RenderSurface> GetAwaiter()
        {
            var hints = _hints;
            return Task.Run(() =>
            {
                return new RenderSurface(
                    new NativeHandle(
                        Platform.Bindings.RequestRenderSurface(
                            _ => { },  // TODO: Implement calls to update delegate
                            hints
                        )
                    )
                );
            }).GetAwaiter();
        }
    }

    internal delegate void NativeRenderSurfaceUpdateDelegate(IntPtr renderContext);

    internal struct RenderSurfaceCreationHints
    {
        public uint Width, Height;
        public string Title;
        public bool Fullscreen;
    }
}