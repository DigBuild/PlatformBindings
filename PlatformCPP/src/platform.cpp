#include "platform.h"

#include "native_handle.h"
#include "utils.h"

namespace digbuild::platform
{
	Platform& Platform::getInstance()
	{
	}
}

using namespace digbuild::platform;
extern "C" {
	DLLEXPORT bool dbp_platform_supports_multiple_render_surfaces()
	{
		return Platform::getInstance().getRenderManager().supportsMultipleRenderSurfaces();
	}
	
	DLLEXPORT native_handle dbp_platform_request_render_surface(
		void(*update)(void* renderContext),
		const RenderSurfaceCreationHints hints
	)
	{
		return make_native_handle(
			Platform::getInstance().getRenderManager().requestRenderSurface(
				[update](RenderContext& ctx)
				{
					update(&ctx);
				},
				hints
			)
		);
	}
}
