#pragma once
#include <memory>
#include <mutex>
#include <vulkan.h>

#include "utils.h"

namespace digbuild::platform::desktop::vulkan
{
	class VulkanContext final : public std::enable_shared_from_this<VulkanContext>
	{
	public:
		explicit VulkanContext(const std::vector<const char*>& surfaceExtensions);
		~VulkanContext() = default;
		VulkanContext(const VulkanContext& other) = delete;
		VulkanContext(VulkanContext&& other) noexcept = delete;
		VulkanContext& operator=(const VulkanContext& other) = delete;
		VulkanContext& operator=(VulkanContext&& other) noexcept = delete;

		[[nodiscard]] bool initializeOrValidateDeviceCompatibility(const vk::SurfaceKHR& surface);
	private:
		[[nodiscard]] bool initializeDevice(const vk::SurfaceKHR& surface);
		[[nodiscard]] bool validateDeviceCompatibility(const vk::SurfaceKHR& surface) const;

	public:
		[[nodiscard]] utils::SwapChainDescriptor getSwapChainDescriptor(const vk::SurfaceKHR& surface) const;

		[[nodiscard]] vk::UniqueSwapchainKHR createSwapChain(
			const vk::SurfaceKHR& surface,
			uint32_t imageCount,
			vk::SurfaceFormatKHR format,
			vk::PresentModeKHR presentMode,
			vk::Extent2D extent,
			vk::SurfaceTransformFlagBitsKHR transform,
			const vk::SwapchainKHR& oldSwapchain
		) const;
		
		[[nodiscard]] const vk::Instance& getInstance() { return *m_instance; }
	
	private:
		std::vector<const char*> m_requiredLayers;
		vk::UniqueInstance m_instance;
		vk::UniqueDebugUtilsMessengerEXT m_debugMessenger;

		std::mutex m_deviceInitLock;
		bool m_deviceInitialized = false;
		std::vector<const char*> m_requiredDeviceExtensions;
		vk::PhysicalDevice m_physicalDevice;
		utils::QueueFamilyIndices m_familyIndices;
		vk::UniqueDevice m_device;

		vk::Queue m_graphicsQueue;
		vk::Queue m_presentQueue;

		vk::UniqueCommandPool m_commandPool;
		vk::UniquePipelineCache m_pipelineCache;
	};
}
