#pragma once
#include <deque>
#include <GLFW.h>
#include <mutex>
#include <thread>
#include <vulkan.h>

#include "dt_context.h"
#include "dt_render_context.h"
#include "../render/render_surface.h"

namespace digbuild::platform::desktop
{
	struct KeyboardEvent
	{
		const uint32_t code;
		const input::KeyboardAction action;
	};
	struct MouseEvent
	{
		const uint32_t button;
		const input::MouseAction action;
	};
	struct CursorEvent
	{
		const uint32_t x, y;
		const input::CursorAction action;
	};
	
	class InputContext final : public input::SurfaceInputContext
	{
	public:
		void consumeKeyboardEvents(input::KeyboardEventConsumer consumer) override;
		void consumeMouseEvents(input::MouseEventConsumer consumer) override;
		void consumeCursorEvents(input::CursorEventConsumer consumer) override;
	
	private:
		std::deque<KeyboardEvent> m_keyboardEvents;
		std::deque<MouseEvent> m_mouseEvents;
		std::deque<CursorEvent> m_cursorEvents;

		friend class RenderSurface;
	};
	
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

		[[nodiscard]] input::SurfaceInputContext& getInputContext() override
		{
			return m_inputContext;
		}

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
		[[nodiscard]] bool isResized() const override
		{
			return m_resized;
		}
		[[nodiscard]] bool wasJustResized() const
		{
			return m_justResized;
		}
		void resetResized()
		{
			m_resized = false;
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

		[[nodiscard]] render::Framebuffer& getFramebuffer() override
		{
			return m_context->getFramebuffer();
		}

	private:
		const GLFWContext& m_glfwContext;
		const std::shared_ptr<RenderSurface> m_parent;
		InputContext m_inputContext;
		std::unique_ptr<RenderContext> m_context;
		uint32_t m_width, m_height;
		std::string m_title;
		bool m_fullscreen, m_visible, m_justResized, m_resized = false;

		bool m_close = false;

		GLFWwindow* m_window;
		std::thread m_updateThread;

		std::mutex m_renderLock;

		friend class GLFWContext;
	};
}
