#pragma once
#include <GLFW.h>
#include <mutex>
#include <thread>
#include <vulkan.h>

#include "context.h"
#include "render_context.h"
#include "../render/render_surface.h"

namespace digbuild::platform::desktop
{
	class RenderSurface final : public render::RenderSurface
	{
	public:
		RenderSurface(
			const GLFWContext& glfwContext,
			std::shared_ptr<RenderSurface>&& parent,
			const RenderContextFactory& contextFactory,
			uint32_t width,
			uint32_t height,
			const std::string& title,
			bool fullscreen
		);

		[[nodiscard]] uint32_t getWidth() const override
		{
			return m_width;
		}
		[[nodiscard]] uint32_t getHeight() const override
		{
			return m_height;
		}
		[[nodiscard]] std::string getTitle() const override
		{
			return m_title;
		}
		[[nodiscard]] bool isFullscreen() const override
		{
			return m_fullscreen;
		}
		[[nodiscard]] bool isVisible() const override
		{
			return m_visible;
		}
		
		void setWidth(uint32_t width) override;
		void setHeight(uint32_t height) override;
		void setTitle(const std::string& title) override;
		void setFullscreen(bool fullscreen) override;
		
		void close() override;
		void waitClosed() override;

		const RenderContext& getContext() const
		{
			return *m_context;
		}

		vk::UniqueSurfaceKHR createVulkanSurface(const vk::Instance& instance) const;
		static std::vector<const char*> getSurfaceExtensions();

	private:
		const GLFWContext& m_glfwContext;
		const std::shared_ptr<RenderSurface> m_parent;
		std::unique_ptr<RenderContext> m_context;
		uint32_t m_width, m_height;
		std::string m_title;
		bool m_fullscreen, m_visible = false;

		bool m_close = false;

		GLFWwindow* m_window;
		std::thread m_updateThread;

		std::mutex m_renderLock;

		friend class GLFWContext;
	};
}
