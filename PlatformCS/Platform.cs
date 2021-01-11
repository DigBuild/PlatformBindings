using System;
using System.Runtime.CompilerServices;

namespace DigBuildPlatformCS
{
    public static class Platform
    {

        public static bool SupportsMultipleRenderSurfaces => throw new NotImplementedException();

        public static RenderSurfaceRequestBuilder RequestRenderSurface(
            RenderSurface.UpdateDelegate update,
            uint widthHint = 800,
            uint heightHint = 600,
            string titleHint = "",
            bool fullscreenHint = false
        ) => throw new NotImplementedException();
    }

    public readonly ref struct RenderSurfaceRequestBuilder
    {

        public RenderSurfaceAwaiter GetAwaiter() => throw new NotImplementedException();
    }

    public sealed class RenderSurfaceAwaiter : INotifyCompletion
    {
        public bool IsCompleted => true;

        public void OnCompleted(Action continuation) => continuation();
        
        public RenderSurface GetResult() => throw new NotImplementedException();
    }
}