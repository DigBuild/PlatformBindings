#pragma once
#include <optional>
#include <set>
#include <vector>
#include <vulkan.h>

namespace digbuild::platform::desktop::vulkan::utils
{
	struct QueueFamilyIndices
	{
		std::optional<uint32_t> graphicsFamily;
		std::optional<uint32_t> presentFamily;

		[[nodiscard]] bool isComplete() const;
		[[nodiscard]] std::set<uint32_t> asSet() const;
	};

	struct SwapChainDescriptor
	{
		vk::SurfaceCapabilitiesKHR capabilities;
		std::vector<vk::SurfaceFormatKHR> formats;
		std::vector<vk::PresentModeKHR> presentModes;

		[[nodiscard]] bool isValid() const;
		[[nodiscard]] vk::SurfaceFormatKHR getOptimalFormat() const;
		[[nodiscard]] vk::PresentModeKHR getOptimalPresentMode() const;
		[[nodiscard]] vk::Extent2D getOptimalExtent(uint32_t width, uint32_t height) const;
		[[nodiscard]] uint32_t getOptimalImageCount() const;
		[[nodiscard]] vk::SurfaceTransformFlagBitsKHR getTransform() const;
	};

	struct PhysicalDeviceDescriptor
	{
		vk::PhysicalDevice device;
		QueueFamilyIndices familyIndices;
	};

	void initializeDispatcher();

	[[nodiscard]] bool areAllLayersAvailable(const std::vector<const char*>& requestedLayers);

	[[nodiscard]] int32_t getPhysicalDeviceScore(const vk::PhysicalDevice& device);
	[[nodiscard]] QueueFamilyIndices findQueueFamilies(const vk::PhysicalDevice& device, const vk::SurfaceKHR& surface);
	[[nodiscard]] bool areAllExtensionsSupported(const vk::PhysicalDevice& device, const std::vector<const char*>& extensions);
	[[nodiscard]] SwapChainDescriptor getSwapChainDescriptor(const vk::PhysicalDevice& device, const vk::SurfaceKHR& surface);
	[[nodiscard]] std::optional<PhysicalDeviceDescriptor> getPhysicalDeviceDescriptor(
		const vk::PhysicalDevice& device,
		const vk::SurfaceKHR& surface,
		const std::vector<const char*>& requiredExtensions
	);
	[[nodiscard]] PhysicalDeviceDescriptor findOptimalPhysicalDevice(
		const vk::Instance& instance,
		const vk::SurfaceKHR& surface,
		const std::vector<const char*>& requiredExtensions
	);

	[[nodiscard]] vk::UniqueDevice createLogicalDevice(
		const vk::PhysicalDevice& physicalDevice,
		const QueueFamilyIndices& familyIndices,
		const std::vector<const char*>& requiredLayers,
		const std::vector<const char*>& requiredExtensions
	);

	[[nodiscard]] vk::UniqueCommandPool createCommandPool(const vk::Device& device, uint32_t graphicsFamily);

	[[nodiscard]] vk::UniquePipelineCache createPipelineCache(const vk::Device& device);
}
