#include "platform.h"

#include "render_context.h"
#include "../render_surface.h"

namespace digbuild::platform::desktop::vulkan
{
	std::shared_ptr<render::RenderSurface> RenderManager::requestRenderSurface(
		render::RenderSurfaceUpdateFunction update, RenderSurfaceCreationHints hints)
	{
		return std::make_shared<RenderSurface>(
			nullptr,
			[](RenderSurface& surface, RenderSurface* parent)
			{
				std::shared_ptr<VulkanContext> context;
				if (parent != nullptr)
					context = dynamic_cast<const RenderContext&>(parent->getContext()).m_context;
				else
					context = std::make_shared<VulkanContext>();
				return std::make_unique<RenderContext>(surface, std::move(context));
			},
			hints.width,
			hints.height,
			hints.title,
			hints.fullscreen
		);
	}
}
