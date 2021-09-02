#pragma once
#include "../dt_context.h"
#include "../dt_global_input_context.h"
#include "../../platform.h"

namespace digbuild::platform::desktop::vulkan
{
	class RenderManager final : public platform::RenderManager
	{
	public:
		RenderManager() :
			m_glfwContext(true) {}
		
		[[nodiscard]] bool supportsMultipleRenderSurfaces() const override
		{
			return true;
		}
		
		[[nodiscard]] std::shared_ptr<render::RenderSurface> requestRenderSurface(
			RenderSurfaceCreationHints hints
		) override;
	
	private:
		GLFWContext m_glfwContext;
	};
	
	class Platform final : public platform::Platform
	{
	public:
		Platform() = default;

		[[nodiscard]] input::GlobalInputContext& getGlobalInputContext() override
		{
			return m_globalInputContext;
		}
		[[nodiscard]] platform::RenderManager& getRenderManager() override
		{
			return m_renderManager;
		}
	private:
		RenderManager m_renderManager;
		GlobalInputContext m_globalInputContext;
	};
}
