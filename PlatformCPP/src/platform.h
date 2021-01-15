#pragma once
#include <memory>

#include "render/render_surface.h"

namespace digbuild::platform
{
	struct RenderSurfaceCreationHints
	{
		uint32_t width, height;
		char* title;
		bool fullscreen;
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
			render::RenderSurfaceUpdateFunction update,
			RenderSurfaceCreationHints hints
		) = 0;
	};
	
	class Platform
	{
	public:
		static Platform& getInstance();
	protected:
		Platform() = default;
	public:
		virtual ~Platform() = default;
		Platform(const Platform& other) = delete;
		Platform(Platform&& other) noexcept = delete;
		Platform& operator=(const Platform& other) = delete;
		Platform& operator=(Platform&& other) noexcept = delete;

		[[nodiscard]] virtual RenderManager& getRenderManager() const = 0;
	};
}
