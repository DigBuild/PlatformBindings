#include "context.h"

#include <iostream>

#include "utils.h"

namespace digbuild::platform::desktop::vulkan
{
	std::vector<const char*> getRequiredLayers()
	{
		return std::vector<const char*>{
#ifdef DB_DEBUG
			"VK_LAYER_KHRONOS_validation"
#endif
		};
	}

	std::vector<const char*> getRequiredInstanceExtensions()
	{
		return std::vector<const char*>{
#ifdef DB_DEBUG
			VK_EXT_DEBUG_UTILS_EXTENSION_NAME
#endif
		};
	}
	
	std::vector<const char*> getRequiredDeviceExtensions()
	{
		return std::vector<const char*>{
			VK_KHR_SWAPCHAIN_EXTENSION_NAME
		};
	}
	
	vk::ApplicationInfo getApplicationInfo()
	{
		return vk::ApplicationInfo{
			"DigBuild", VK_MAKE_VERSION(1, 0, 0),
			"DigBuild", VK_MAKE_VERSION(1, 0, 0),
			VK_API_VERSION_1_2
		};
	}

	VKAPI_ATTR VkBool32 VKAPI_CALL logDebugMessage(
		VkDebugUtilsMessageSeverityFlagBitsEXT messageSeverity,
		const VkDebugUtilsMessageTypeFlagsEXT messageTypes,
		VkDebugUtilsMessengerCallbackDataEXT const* pCallbackData,
		void* /*pUserData*/)
	{
		std::cerr << vk::to_string(static_cast<vk::DebugUtilsMessageSeverityFlagBitsEXT>(messageSeverity)) << ": "
			<< vk::to_string(static_cast<vk::DebugUtilsMessageTypeFlagsEXT>(messageTypes)) << ":\n";
		std::cerr << "\t"
			<< "messageIDName   = <" << pCallbackData->pMessageIdName << ">\n";
		std::cerr << "\t"
			<< "messageIdNumber = " << pCallbackData->messageIdNumber << "\n";
		std::cerr << "\t"
			<< "message         = <" << pCallbackData->pMessage << ">" << std::endl;
		return true;
	}
	
	VulkanContext::VulkanContext(const std::vector<const char*>& surfaceExtensions)
	{
		utils::initializeDispatcher();

		m_requiredLayers = getRequiredLayers();
		if (!utils::areAllLayersAvailable(m_requiredLayers))
			throw std::runtime_error("Not all requested layers are available.");

		const auto instanceExtensions = getRequiredInstanceExtensions();
		
		std::vector<const char*> requiredExtensions;
		requiredExtensions.reserve(surfaceExtensions.size() + instanceExtensions.size());
		requiredExtensions.insert(requiredExtensions.end(), surfaceExtensions.begin(), surfaceExtensions.end());
		requiredExtensions.insert(requiredExtensions.end(), instanceExtensions.begin(), instanceExtensions.end());

		const auto appInfo = getApplicationInfo();
		m_instance = vk::createInstanceUnique(vk::InstanceCreateInfo{
				{}, &appInfo,
				static_cast<uint32_t>(m_requiredLayers.size()), m_requiredLayers.data(),
				static_cast<uint32_t>(requiredExtensions.size()), requiredExtensions.data()
		});
		VULKAN_HPP_DEFAULT_DISPATCHER.init(*m_instance);

#ifdef DB_DEBUG
		m_debugMessenger = m_instance->createDebugUtilsMessengerEXTUnique(
			vk::DebugUtilsMessengerCreateInfoEXT(
				{},
				vk::DebugUtilsMessageSeverityFlagBitsEXT::eWarning |
				vk::DebugUtilsMessageSeverityFlagBitsEXT::eError,
				vk::DebugUtilsMessageTypeFlagBitsEXT::eGeneral |
				vk::DebugUtilsMessageTypeFlagBitsEXT::ePerformance |
				vk::DebugUtilsMessageTypeFlagBitsEXT::eValidation,
				&logDebugMessage
			)
		);
#endif
	}

	bool VulkanContext::initializeOrValidateDeviceCompatibility(const vk::SurfaceKHR& surface)
	{
		m_deviceInitLock.lock();
		if (m_deviceInitialized)
		{
			m_deviceInitLock.unlock();
			return validateDeviceCompatibility(surface);
		}
		const auto result = initializeDevice(surface);
		m_deviceInitLock.unlock();
		return result;
	}

	bool VulkanContext::initializeDevice(const vk::SurfaceKHR& surface)
	{
		m_requiredDeviceExtensions = getRequiredDeviceExtensions();
		
		const auto deviceDescriptor = utils::findOptimalPhysicalDevice(*m_instance, surface, m_requiredDeviceExtensions);
		m_physicalDevice = deviceDescriptor.device;
		m_familyIndices = deviceDescriptor.familyIndices;

		m_device = utils::createLogicalDevice(m_physicalDevice, m_familyIndices, m_requiredLayers, m_requiredDeviceExtensions);
		
		m_graphicsQueue = m_device->getQueue(m_familyIndices.graphicsFamily.value(), 0);
		m_presentQueue = m_device->getQueue(m_familyIndices.presentFamily.value(), 0);
		
		m_commandPool = utils::createCommandPool(*m_device, m_familyIndices.graphicsFamily.value());
		m_pipelineCache = utils::createPipelineCache(*m_device);

		m_deviceInitialized = true;
		return true;
	}

	bool VulkanContext::validateDeviceCompatibility(const vk::SurfaceKHR& surface) const
	{
		return utils::getPhysicalDeviceDescriptor(m_physicalDevice, surface, m_requiredDeviceExtensions).has_value();
	}

	utils::SwapChainDescriptor VulkanContext::getSwapChainDescriptor(const vk::SurfaceKHR& surface) const
	{
		return utils::getSwapChainDescriptor(m_physicalDevice, surface);
	}

	vk::UniqueSwapchainKHR VulkanContext::createSwapChain(
		const vk::SurfaceKHR& surface,
		const uint32_t imageCount,
		const vk::SurfaceFormatKHR format,
		const vk::PresentModeKHR presentMode,
		const vk::Extent2D extent,
		const vk::SurfaceTransformFlagBitsKHR transform,
		const vk::SwapchainKHR& oldSwapchain
	) const
	{
		vk::SwapchainCreateInfoKHR swapchainCreateInfo(
			{}, surface, imageCount, format.format, format.colorSpace,
			extent, 1, vk::ImageUsageFlagBits::eColorAttachment,
			vk::SharingMode::eExclusive, 0, nullptr, transform,
			vk::CompositeAlphaFlagBitsKHR::eOpaque, presentMode, true, oldSwapchain
		);
		uint32_t queueFamilyIndices[] = { m_familyIndices.graphicsFamily.value(), m_familyIndices.presentFamily.value() };
		if (m_familyIndices.graphicsFamily != m_familyIndices.presentFamily)
		{
			swapchainCreateInfo.imageSharingMode = vk::SharingMode::eConcurrent;
			swapchainCreateInfo.queueFamilyIndexCount = 2;
			swapchainCreateInfo.pQueueFamilyIndices = queueFamilyIndices;
		}
		return m_device->createSwapchainKHRUnique(swapchainCreateInfo);
	}
}
