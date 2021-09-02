using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdvancedDLSupport;
using DigBuild.Platform.Audio;
using DigBuild.Platform.Input;
using DigBuild.Platform.Render;
using DigBuild.Platform.Util;

namespace DigBuild.Platform
{
    [NativeSymbols("dbp_platform_", SymbolTransformationMethod.Underscore)]
    internal interface IPlatformBindings
    {
        IntPtr GetGlobalInputContext();

        bool SupportsMultipleRenderSurfaces();

        IntPtr RequestRenderSurface(
            RenderSurfaceCreationHints hints,
            IntPtr parent
        );
    }

    public static class Platform
    {
        internal static readonly IPlatformBindings Bindings = NativeLib.Get<IPlatformBindings>();

        private static readonly Lazy<AudioSystem> LazyAudioSystem = new(() => new AudioSystem());

        public static GlobalInputContext InputContext { get; } = new(Bindings.GetGlobalInputContext());

        public static AudioSystem AudioSystem => LazyAudioSystem.Value;

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
            return new RenderSurfaceRequestBuilder(
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
            var parent = _parent;

            var task = new TaskCompletionSource<RenderSurface>();

            new Thread(() =>
            {
                var forceStop = false;

                var handle = new NativeHandle(
                    Platform.Bindings.RequestRenderSurface(
                        hints,
                        parent?.Handle ?? NativeHandle.Empty
                    )
                );

                var surface = new RenderSurface(handle, () => forceStop = true);
                task.SetResult(surface);

                var surfaceContext = new RenderSurfaceContext(handle);

                while (!forceStop && surface.Active)
                {
                    var context = surface.UpdateFirst(out var skip);
                    if (skip) continue;
                    updateFunc(surfaceContext, context);
                    surface.UpdateLast();
                }

                surface.Terminate(forceStop);

            }) { Name = "Render" }.Start();

            return task.Task.GetAwaiter();
        }
    }
    
    internal struct RenderSurfaceCreationHints
    {
        public uint Width, Height;
        public string Title;
        public bool Fullscreen;
        public bool FallbackOnIncompatibleParent;
    }
}