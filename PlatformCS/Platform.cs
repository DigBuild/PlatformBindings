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
            RenderSurface? parent = null,
            bool fallbackOnIncompatibleParentHint = false,
            uint widthHint = 800,
            uint heightHint = 600,
            string titleHint = "",
            bool fullscreenHint = false
        )
        {
            return new(
                parent,
                update,
                new RenderSurfaceCreationHints
                {
                    Width = widthHint,
                    Height = heightHint,
                    Title = titleHint,
                    Fullscreen = fullscreenHint,
                    FallbackOnIncompatibleParent = fallbackOnIncompatibleParentHint
                }
            );
        }
    }

    public readonly ref struct RenderSurfaceRequestBuilder
    {
        private readonly RenderSurface? _parent;
        private readonly RenderSurface.UpdateDelegate _update;
        private readonly RenderSurfaceCreationHints _hints;

        internal RenderSurfaceRequestBuilder(RenderSurface? parent, RenderSurface.UpdateDelegate update, RenderSurfaceCreationHints hints)
        {
            _parent = parent;
            _update = update;
            _hints = hints;
        }

        public TaskAwaiter<RenderSurface> GetAwaiter()
        {
            var hints = _hints;
            var updateFunc = _update;
            return Task.Run(() =>
            {
                return new RenderSurface(
                    new NativeHandle(
                        Platform.Bindings.RequestRenderSurface(
                            (renderSurfaceContextPtr, renderContextPtr) =>
                                updateFunc(new RenderSurfaceContext(renderSurfaceContextPtr), new RenderContext(renderContextPtr)),
                            hints
                        )
                    ),
                    hints.Title
                );
            }).GetAwaiter();
        }
    }

    internal delegate void NativeRenderSurfaceUpdateDelegate(IntPtr renderSurfaceContext, IntPtr renderContext);

    internal struct RenderSurfaceCreationHints
    {
        public uint Width, Height;
        public string Title;
        public bool Fullscreen;
        public bool FallbackOnIncompatibleParent;
    }
}