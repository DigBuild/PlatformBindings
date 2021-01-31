#include "platform.h"

#include "desktop/vulkan/platform.h"
#include "util/native_handle.h"
#include "util/utils.h"

namespace digbuild::platform
{
	Platform& Platform::getInstance()
	{
		static desktop::vulkan::Platform instance;
		return instance;
	}
}

using namespace digbuild::platform;
extern "C" {
	DLLEXPORT bool dbp_platform_supports_multiple_render_surfaces()
	{
		return Platform::getInstance().getRenderManager().supportsMultipleRenderSurfaces();
	}
	
	DLLEXPORT util::native_handle dbp_platform_request_render_surface(
		void(*update)(const void* surfaceContext, const void* renderContext),
		const RenderSurfaceCreationHints hints
	)
	{
		return util::make_native_handle(
			Platform::getInstance().getRenderManager().requestRenderSurface(
				[update](const render::RenderSurface& surface, const render::RenderContext& ctx)
				{
					update(&surface, &ctx);
				},
				hints
			)
		);
	}
}
