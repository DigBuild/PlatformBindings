﻿#include "dt_render_surface.h"
#include <atomic>

namespace digbuild::platform::desktop
{
	void InputContext::consumeKeyboardEvents(const input::KeyboardEventConsumer consumer)
	{
		while (!m_keyboardEvents.empty())
		{
			auto& evt = m_keyboardEvents.front();
			consumer(evt.code, evt.action);
			m_keyboardEvents.pop_front();
		}
	}

	void InputContext::consumeMouseEvents(const input::MouseEventConsumer consumer)
	{
		while (!m_mouseEvents.empty())
		{
			auto& evt = m_mouseEvents.front();
			consumer(evt.button, evt.action);
			m_mouseEvents.pop_front();
		}
	}

	void InputContext::consumeScrollEvents(const input::ScrollEventConsumer consumer)
	{
		while (!m_scrollEvents.empty())
		{
			auto& evt = m_scrollEvents.front();
			consumer(evt.xOffset, evt.yOffset);
			m_scrollEvents.pop_front();
		}
	}


	void InputContext::consumeCursorEvents(const input::CursorEventConsumer consumer)
	{
		while (!m_cursorEvents.empty())
		{
			auto& evt = m_cursorEvents.front();
			consumer(evt.x, evt.y, evt.action);
			m_cursorEvents.pop_front();
		}
	}

	void InputContext::setCursorMode(const input::CursorMode mode)
	{
		m_cursorMode = mode;
		glfwSetInputMode(
			m_surface->m_window,
			GLFW_CURSOR, 
			mode == input::CursorMode::HIDDEN ? GLFW_CURSOR_HIDDEN :
			mode == input::CursorMode::RAW ? GLFW_CURSOR_DISABLED :
			GLFW_CURSOR_NORMAL
		);
	}

	void InputContext::centerCursor()
	{
		glfwSetCursorPos(m_surface->m_window, m_surface->m_width / 2, m_surface->m_height / 2);
	}

	RenderSurface::RenderSurface(
		const GLFWContext& glfwContext,
		std::shared_ptr<RenderSurface>&& parent,
		const RenderContextFactory& contextFactory,
		const uint32_t width,
		const uint32_t height,
		const std::string& title,
		const bool fullscreen
	) :
		m_glfwContext(glfwContext),
		m_parent(std::move(parent)),
		m_inputContext(InputContext(this)),
		m_width(width),
		m_height(height),
		m_title(title),
		m_fullscreen(fullscreen)
	{
		GLFWmonitor* monitor = nullptr;
		if (fullscreen)
			monitor = glfwGetPrimaryMonitor();

		GLFWwindow* share = nullptr;
		if (m_parent != nullptr)
			share = m_parent->m_window;

		m_window = glfwCreateWindow(width, height, title.c_str(), monitor, share);
		glfwSetWindowUserPointer(m_window, this);

		// Create the context once we have a window
		m_context = contextFactory(*this, m_parent.get());

		if (glfwRawMouseMotionSupported())
			glfwSetInputMode(m_window, GLFW_RAW_MOUSE_MOTION, GLFW_TRUE);

		glfwSetFramebufferSizeCallback(
			m_window,
			[](GLFWwindow* win, const int width, const int height)
			{
				auto* window = static_cast<RenderSurface*>(glfwGetWindowUserPointer(win));

				// [[maybe_unused]] std::scoped_lock<std::mutex> lock(window->m_renderLock);

				window->m_width = width;
				window->m_height = height;
				window->m_visible = width != 0 && height != 0;
				window->m_justResized = true;
				window->m_resized = true;
			}
		);

		glfwSetKeyCallback(
			m_window,
			[](GLFWwindow* win, int, const int scancode, const int action, int)
			{
				auto* window = static_cast<RenderSurface*>(glfwGetWindowUserPointer(win));

				window->m_inputContext.m_keyboardEvents.push_back({
					static_cast<uint32_t>(scancode),
					static_cast<input::KeyboardAction>(action)
				});
			}
		);
		
		glfwSetCharCallback(
			m_window,
			[](GLFWwindow* win, const unsigned int c)
			{
				auto* window = static_cast<RenderSurface*>(glfwGetWindowUserPointer(win));

				window->m_inputContext.m_keyboardEvents.push_back({
					c,
					input::KeyboardAction::CHARACTER
				});
			}
		);
		
		glfwSetMouseButtonCallback(
			m_window,
			[](GLFWwindow* win, const int button, const int action, int)
			{
				auto* window = static_cast<RenderSurface*>(glfwGetWindowUserPointer(win));

				window->m_inputContext.m_mouseEvents.push_back({
					static_cast<uint32_t>(button),
					static_cast<input::MouseAction>(action)
				});
			}
		);

		glfwSetScrollCallback(
			m_window,
			[](GLFWwindow* win, const double xOff, const double yOff)
			{
				auto* window = static_cast<RenderSurface*>(glfwGetWindowUserPointer(win));

				window->m_inputContext.m_scrollEvents.push_back({ xOff, yOff });
			}
		);

		glfwSetCursorPosCallback(
			m_window,
			[](GLFWwindow* win, const double x, const double y)
			{
				auto* window = static_cast<RenderSurface*>(glfwGetWindowUserPointer(win));

				window->m_inputContext.m_cursorEvents.push_back({
					static_cast<uint32_t>(x),
					static_cast<uint32_t>(y),
					input::CursorAction::MOVE
				});
			}
		);

		// TODO: Support IME and alternative input methods
		// glfwSetCharCallback(
		// 	m_window,
		// 	[](GLFWwindow* win, unsigned int character)
		// 	{
		// 	}
		// );
	}

	bool RenderSurface::isActive() const
	{
		return !m_close && !glfwWindowShouldClose(m_window);
	}

	render::RenderContext* RenderSurface::updateFirst()
	{
		if (m_width == 0 || m_height == 0)
			glfwWaitEvents();
		else
			glfwPollEvents();
		
		if (m_width == 0 || m_height == 0)
			return nullptr;

		m_context->updateFirst();
		
		return m_context.get();
	}
	
	void RenderSurface::updateLast()
	{
		m_context->updateLast();

		m_justResized = false;
	}

	void RenderSurface::terminate(bool force)
	{
		// Manually terminate the context before the window closes
		m_context = nullptr;

		glfwDestroyWindow(m_window);
	}
	
	void RenderSurface::setWidth(const uint32_t width)
	{
		m_width = width;
		glfwSetWindowSize(m_window, m_width, m_height);
	}

	void RenderSurface::setHeight(const uint32_t height)
	{
		m_height = height;
		glfwSetWindowSize(m_window, m_width, m_height);
	}

	void RenderSurface::setTitle(const std::string& title)
	{
		m_title = title;
		glfwSetWindowTitle(m_window, m_title.c_str());
	}

	void RenderSurface::setFullscreen(const bool fullscreen)
	{
		// TODO: Implement fullscreen support
	}

	vk::UniqueSurfaceKHR RenderSurface::createVulkanSurface(const vk::Instance& instance) const
	{
		VkSurfaceKHR tmpSurface;
		if (glfwCreateWindowSurface(instance, m_window, nullptr, &tmpSurface) != VK_SUCCESS)
			throw std::runtime_error("Failed to create window surface.");

		return vk::UniqueSurfaceKHR(
			reinterpret_cast<vk::SurfaceKHR&>(tmpSurface),
			vk::ObjectDestroy<vk::Instance, vk::DispatchLoaderDynamic>(instance)
		);
	}

	std::vector<const char*> RenderSurface::getSurfaceExtensions()
	{
		uint32_t glfwExtensionCount;
		auto* const glfwExtensionPtr = glfwGetRequiredInstanceExtensions(&glfwExtensionCount);
		return std::vector<const char*>(glfwExtensionPtr, glfwExtensionPtr + glfwExtensionCount);
	}
}
