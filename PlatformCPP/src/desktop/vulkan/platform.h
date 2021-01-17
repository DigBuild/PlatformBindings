#pragma once
#include "../../platform.h"

namespace digbuild::platform::desktop::vulkan
{
	class RenderManager final : public platform::RenderManager
	{
	public:
		[[nodiscard]] bool supportsMultipleRenderSurfaces() const override
		{
			return true;
		}
		
		[[nodiscard]] std::shared_ptr<render::RenderSurface> requestRenderSurface(
			render::RenderSurfaceUpdateFunction update,
			RenderSurfaceCreationHints hints
		) override;
	};
	
	class Platform final : public platform::Platform
	{
	public:
		Platform() = default;

		[[nodiscard]] platform::RenderManager& getRenderManager() override
		{
			return m_renderManager;
		}
	private:
		RenderManager m_renderManager;
	};
}
