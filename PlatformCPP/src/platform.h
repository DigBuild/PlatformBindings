#pragma once
#include <memory>

#include "input/global_input_context.h"
#include "render/render_surface.h"

namespace digbuild::platform
{
	struct RenderSurfaceCreationHints
	{
		uint32_t width, height;
		char* title;
		bool fullscreen;
		bool fallbackOnIncompatibleParent;
	};
	
	class RenderManager
	{
	public:
		RenderManager() = default;
		virtual ~RenderManager() = default;
		RenderManager(const RenderManager& other) = delete;
		RenderManager(RenderManager&& other) noexcept = delete;
		RenderManager& operator=(const RenderManager& other) = delete;
		RenderManager& operator=(RenderManager&& other) noexcept = delete;

		[[nodiscard]] virtual bool supportsMultipleRenderSurfaces() const = 0;
		[[nodiscard]] virtual std::shared_ptr<render::RenderSurface> requestRenderSurface(
			RenderSurfaceCreationHints hints
		) = 0;
	};
	
	class Platform
	{
	public:
		static Platform& getInstance();
		Platform() = default;
		virtual ~Platform() = default;
		Platform(const Platform& other) = delete;
		Platform(Platform&& other) noexcept = delete;
		Platform& operator=(const Platform& other) = delete;
		Platform& operator=(Platform&& other) noexcept = delete;
		
		[[nodiscard]] virtual input::GlobalInputContext& getGlobalInputContext() = 0;
		[[nodiscard]] virtual RenderManager& getRenderManager() = 0;
	};
}
