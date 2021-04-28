#pragma once
#include <optional>
#include <set>
#include <vector>
#include <vulkan.h>

#include "../../render/texture.h"

namespace digbuild::platform::desktop::vulkan::util
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

	struct ImageTransitionInfo
	{
		vk::Image image;
		vk::ImageAspectFlags aspectFlags;
		vk::ImageLayout oldLayout;
		vk::ImageLayout newLayout;
	};

	struct ImageMemoryBarrierSet
	{
		const std::vector<vk::ImageMemoryBarrier> barriers;
		const vk::PipelineStageFlags srcStageMask;
		const vk::PipelineStageFlags dstStageMask;
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

	void directExecuteCommands(
		const vk::Device& device,
		const vk::CommandPool& commandPool,
		const vk::Queue& queue,
		const std::function<void(vk::CommandBuffer&)>& commands
	);

	ImageMemoryBarrierSet createImageMemoryBarriers(
		const std::vector<ImageTransitionInfo>& transitions
	);
	
	void transitionImageLayouts(
		const vk::CommandBuffer& cmd,
		const std::vector<ImageTransitionInfo>& transitions
	);

	void transitionImageLayoutsImmediate(
		const vk::Device& device,
		const vk::CommandPool& commandPool,
		const vk::Queue& graphicsQueue,
		const std::vector<ImageTransitionInfo>& transitions
	);

	void copyBufferToBuffer(
		const vk::CommandBuffer& cmd,
		const vk::Buffer& src,
		const vk::Buffer& dst,
		const uint32_t size
	);

	void copyBufferToBufferImmediate(
		const vk::Device& device,
		const vk::CommandPool& commandPool,
		const vk::Queue& graphicsQueue,
		const vk::Buffer& src,
		const vk::Buffer& dst,
		const uint32_t size
	);

	void copyBufferToImage(
		const vk::CommandBuffer& cmd,
		const vk::Buffer& buffer,
		const vk::Image& image,
		uint32_t width,
		uint32_t height
	);

	void copyBufferToImageImmediate(
		const vk::Device& device,
		const vk::CommandPool& commandPool,
		const vk::Queue& graphicsQueue,
		const vk::Buffer& buffer,
		const vk::Image& image,
		uint32_t width,
		uint32_t height
	);

	[[nodiscard]] vk::Format toVulkanFormat(render::TextureFormat format);

	[[nodiscard]] uint32_t findMemoryType(const vk::PhysicalDevice& device, uint32_t memoryTypeBits, vk::MemoryPropertyFlags memoryProperties);
}
