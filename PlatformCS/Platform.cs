using System;
using System.Threading.Tasks;

namespace DigBuildPlatformCS
{
    public static class Platform
    {

        public static bool SupportsMultipleRenderSurfaces => throw new NotImplementedException();

        public static Task<RenderSurface> RequestRenderSurface(
            RenderSurface.UpdateDelegate update,
            uint widthHint = 800,
            uint heightHint = 600,
            string titleHint = "",
            bool fullscreenHint = false
        ) => throw new NotImplementedException();
    }
}