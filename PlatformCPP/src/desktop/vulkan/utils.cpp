#include "utils.h"

#include <map>
#include <vulkan.h>

VULKAN_HPP_DEFAULT_DISPATCH_LOADER_DYNAMIC_STORAGE

namespace digbuild::platform::desktop::vulkan::utils
{
	bool QueueFamilyIndices::isComplete() const
	{
		return
			graphicsFamily.has_value() &&
			presentFamily.has_value();
	}

	std::set<uint32_t> QueueFamilyIndices::asSet() const
	{
		std::set<uint32_t> set{
			graphicsFamily.has_value() ? graphicsFamily.value() : 0xFFFFFFFF,
			presentFamily.has_value() ? presentFamily.value() : 0xFFFFFFFF,
		};
		set.erase(0xFFFFFFFF);
		return set;
	}

	bool SwapChainDescriptor::isValid() const
	{
		return !formats.empty() && !presentModes.empty();
	}

	vk::SurfaceFormatKHR SwapChainDescriptor::getOptimalFormat() const
	{
		for (const auto& availableFormat : formats)
			if (availableFormat.format == vk::Format::eB8G8R8A8Srgb &&
				availableFormat.colorSpace == vk::ColorSpaceKHR::eSrgbNonlinear)
				return availableFormat;
		return formats[0];
	}

	vk::PresentModeKHR SwapChainDescriptor::getOptimalPresentMode() const
	{
		for (const auto& availablePresentMode : presentModes)
			if (availablePresentMode == vk::PresentModeKHR::eMailbox)
				return availablePresentMode;
		return vk::PresentModeKHR::eFifo;
	}

	vk::Extent2D SwapChainDescriptor::getOptimalExtent(const uint32_t width, const uint32_t height) const
	{
		if (capabilities.currentExtent.width != UINT32_MAX)
			return capabilities.currentExtent;

		return vk::Extent2D{
			std::max(capabilities.minImageExtent.width, std::min(capabilities.maxImageExtent.width, width)),
			std::max(capabilities.minImageExtent.height, std::min(capabilities.maxImageExtent.height, height))
		};
	}

	uint32_t SwapChainDescriptor::getOptimalImageCount() const
	{
		const auto count = capabilities.minImageCount + 1;
		if (capabilities.maxImageCount > 0 && capabilities.maxImageCount < count)
			return capabilities.maxImageCount;
		return count;
	}

	vk::SurfaceTransformFlagBitsKHR SwapChainDescriptor::getTransform() const
	{
		return capabilities.currentTransform;
	}

	void initializeDispatcher()
	{
		const vk::DynamicLoader dl;
		const auto vkGetInstanceProcAddr = dl.getProcAddress<PFN_vkGetInstanceProcAddr>("vkGetInstanceProcAddr");
		VULKAN_HPP_DEFAULT_DISPATCHER.init(vkGetInstanceProcAddr);
	}

	bool areAllLayersAvailable(const std::vector<const char*>& requestedLayers)
	{
		auto availableLayers = vk::enumerateInstanceLayerProperties();

		for (const auto* requested : requestedLayers)
		{
			for (auto available : availableLayers)
				if (strcmp(requested, available.layerName.data()) == 0)
					goto found;

			return false;
		found:;
		}

		return true;
	}

	int32_t getPhysicalDeviceScore(const vk::PhysicalDevice& device)
	{
		auto score = 0;

		const auto props = device.getProperties();

		score += 1000 * (props.deviceType == vk::PhysicalDeviceType::eDiscreteGpu);
		score += 100 * (props.deviceType == vk::PhysicalDeviceType::eIntegratedGpu);

		return score;
	}

	QueueFamilyIndices findQueueFamilies(const vk::PhysicalDevice& device, const vk::SurfaceKHR& surface)
	{
		QueueFamilyIndices indices;

		auto queueFamilies = device.getQueueFamilyProperties();
		uint32_t i = 0;
		for (const auto& queueFamily : queueFamilies)
		{
			if (queueFamily.queueFlags & vk::QueueFlagBits::eGraphics)
				indices.graphicsFamily = i;

			if (device.getSurfaceSupportKHR(i, surface))
				indices.presentFamily = i;

			i++;
		}

		return indices;
	}

	bool areAllExtensionsSupported(const vk::PhysicalDevice& device, const std::vector<const char*>& extensions)
	{
		auto availableExtensions = device.enumerateDeviceExtensionProperties();
		std::set<std::string> requiredExtensions(extensions.begin(), extensions.end());
		for (const auto& extension : availableExtensions)
		{
			requiredExtensions.erase(extension.extensionName);
		}
		return requiredExtensions.empty();
	}

	SwapChainDescriptor getSwapChainDescriptor(const vk::PhysicalDevice& device, const vk::SurfaceKHR& surface)
	{
		return SwapChainDescriptor{
			device.getSurfaceCapabilitiesKHR(surface),
			device.getSurfaceFormatsKHR(surface),
			device.getSurfacePresentModesKHR(surface)
		};
	}

	std::optional<PhysicalDeviceDescriptor> getPhysicalDeviceDescriptor(
		const vk::PhysicalDevice& device,
		const vk::SurfaceKHR& surface,
		const std::vector<const char*>& requiredExtensions
	)
	{
		const auto indices = findQueueFamilies(device, surface);
		if (!indices.isComplete())
			return {};

		if (!areAllExtensionsSupported(device, requiredExtensions))
			return {};

		const auto swapChainDescriptor = getSwapChainDescriptor(device, surface);
		if (!swapChainDescriptor.isValid())
			return {};

		return PhysicalDeviceDescriptor{ device, indices };
	}

	PhysicalDeviceDescriptor findOptimalPhysicalDevice(
		const vk::Instance& instance,
		const vk::SurfaceKHR& surface,
		const std::vector<const char*>& requiredExtensions
	)
	{
		auto devices = instance.enumeratePhysicalDevices();
		std::multimap<int32_t, PhysicalDeviceDescriptor> candidates;

		for (auto device : devices)
		{
			const auto descriptor = getPhysicalDeviceDescriptor(device, surface, requiredExtensions);
			if (descriptor.has_value())
				candidates.emplace(getPhysicalDeviceScore(device), descriptor.value());
		}

		if (candidates.empty() || candidates.rbegin()->first <= 0)
			throw std::runtime_error("Failed to find a suitable GPU.");

		return candidates.rbegin()->second;
	}

	vk::UniqueDevice createLogicalDevice(
		const vk::PhysicalDevice& physicalDevice,
		const QueueFamilyIndices& familyIndices,
		const std::vector<const char*>& requiredLayers,
		const std::vector<const char*>& requiredExtensions
	)
	{
		auto uniqueFamilyIndices = familyIndices.asSet();
		const auto queuePriority = 1.0f;
		std::vector<vk::DeviceQueueCreateInfo> deviceQueueCreateInfos;
		deviceQueueCreateInfos.reserve(uniqueFamilyIndices.size());
		for (auto index : uniqueFamilyIndices)
			deviceQueueCreateInfos.push_back(vk::DeviceQueueCreateInfo({}, index, 1, &queuePriority));

		vk::PhysicalDeviceFeatures deviceFeatures;
		const vk::DeviceCreateInfo deviceCreateInfo(
			{},
			static_cast<uint32_t>(deviceQueueCreateInfos.size()), deviceQueueCreateInfos.data(),
			static_cast<uint32_t>(requiredLayers.size()), requiredLayers.data(),
			static_cast<uint32_t>(requiredExtensions.size()), requiredExtensions.data(),
			&deviceFeatures
		);

		auto device = physicalDevice.createDeviceUnique(deviceCreateInfo);
		VULKAN_HPP_DEFAULT_DISPATCHER.init(*device);
		return device;
	}

	vk::UniqueCommandPool createCommandPool(const vk::Device& device, const uint32_t graphicsFamily)
	{
		return device.createCommandPoolUnique(vk::CommandPoolCreateInfo{
			vk::CommandPoolCreateFlagBits::eResetCommandBuffer,
			graphicsFamily
		});
	}

	vk::UniquePipelineCache createPipelineCache(const vk::Device& device)
	{
		return device.createPipelineCacheUnique(vk::PipelineCacheCreateInfo{
			{},
			0, nullptr
		});
	}
}
