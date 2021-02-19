#include "vk_platform.h"

#include "vk_render_context.h"
#include "../dt_render_surface.h"

namespace digbuild::platform::desktop::vulkan
{
	std::shared_ptr<render::RenderSurface> RenderManager::requestRenderSurface(
		const render::RenderSurfaceUpdateFunction& update,
		RenderSurfaceCreationHints hints
	)
	{
		return std::make_shared<RenderSurface>(
			m_glfwContext,
			nullptr,
			[update, hints](RenderSurface& surface, RenderSurface* parent)
			{
				std::shared_ptr<VulkanContext> context;
				if (parent != nullptr)
					context = dynamic_cast<const RenderContext&>(parent->getContext()).m_context;
				else
					context = std::make_shared<VulkanContext>(RenderSurface::getSurfaceExtensions());

				auto vkSurface = surface.createVulkanSurface(context->getInstance());
				if (!context->initializeOrValidateDeviceCompatibility(*vkSurface))
				{
					if (parent == nullptr)
						throw std::runtime_error("Could not create Vulkan context.");
					
					if (!hints.fallbackOnIncompatibleParent)
						throw std::runtime_error("Incompatible parent surface. Cannot share Vulkan context.");
					
					context = std::make_shared<VulkanContext>(RenderSurface::getSurfaceExtensions());
					if (!context->initializeOrValidateDeviceCompatibility(*vkSurface))
						throw std::runtime_error("Incompatible parent surface. Could not create fallback Vulkan context.");
				}

				return std::make_unique<RenderContext>(surface, std::move(context), std::move(vkSurface), update);
			},
			hints.width,
			hints.height,
			hints.title,
			hints.fullscreen
		);
	}
}
