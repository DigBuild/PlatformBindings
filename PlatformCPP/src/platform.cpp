#include "platform.h"

#include "desktop/vulkan/vk_platform.h"
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
	DLLEXPORT void* dbp_platform_get_global_input_context()
	{
		return &Platform::getInstance().getGlobalInputContext();
	}
	
	DLLEXPORT bool dbp_platform_supports_multiple_render_surfaces()
	{
		return Platform::getInstance().getRenderManager().supportsMultipleRenderSurfaces();
	}
	
	DLLEXPORT util::native_handle dbp_platform_request_render_surface(
		void(*update)(util::native_handle surfaceContext, const void* renderContext),
		const RenderSurfaceCreationHints hints
	)
	{
		return util::make_native_handle(
			Platform::getInstance().getRenderManager().requestRenderSurface(
				[update](const render::RenderSurface& surface, const render::RenderContext& ctx)
				{
					update(util::make_native_handle(surface.shared_from_this()), &ctx);
				},
				hints
			)
		);
	}
}
