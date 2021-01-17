#include "render_surface.h"

namespace digbuild::platform::desktop
{
	RenderSurface::RenderSurface(
		std::shared_ptr<RenderSurface>&& parent,
		const RenderContextFactory& contextFactory,
		const uint32_t width,
		const uint32_t height,
		const std::string& title,
		const bool fullscreen
	) :
		m_parent(std::move(parent)),
		m_width(width),
		m_height(height),
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

		m_context = contextFactory(*this, m_parent.get());

		glfwSetFramebufferSizeCallback(
			m_window,
			[](GLFWwindow* win, const int width, const int height)
			{
				auto* window = static_cast<RenderSurface*>(glfwGetWindowUserPointer(win));
				
				[[maybe_unused]] std::scoped_lock<std::mutex> lock(window->m_renderLock);

				window->m_width = width;
				window->m_height = height;
			}
		);

		m_updateThread = std::thread(
			[&]()
			{
				while (!m_close && !glfwWindowShouldClose(m_window))
				{
					[[maybe_unused]] std::scoped_lock<std::mutex> lock(m_renderLock);
					
					// TODO: Actually render stuff
				}

				m_context = nullptr; // Manually terminate the context once the window closes

				glfwDestroyWindow(m_window);
			}
		);
	}

	void RenderSurface::close()
	{
		m_close = true;
	}

	void RenderSurface::waitClosed()
	{
		if (m_updateThread.joinable())
			m_updateThread.join();
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
