#pragma once
#include <memory>
#include <mutex>
#include <vulkan.h>

#include "staging_resource.h"
#include "utils.h"

namespace digbuild::platform::desktop::vulkan
{
	struct RenderPassAttachment
	{
		vk::Format format;
		vk::ImageLayout targetLayout;
	};
	
	class VulkanContext final : public std::enable_shared_from_this<VulkanContext>
	{
	public:
		explicit VulkanContext(const std::vector<const char*>& surfaceExtensions);
		~VulkanContext();
		VulkanContext(const VulkanContext& other) = delete;
		VulkanContext(VulkanContext&& other) noexcept = delete;
		VulkanContext& operator=(const VulkanContext& other) = delete;
		VulkanContext& operator=(VulkanContext&& other) noexcept = delete;

		[[nodiscard]] bool initializeOrValidateDeviceCompatibility(const vk::SurfaceKHR& surface);
	private:
		[[nodiscard]] bool initializeDevice(const vk::SurfaceKHR& surface);
		[[nodiscard]] bool validateDeviceCompatibility(const vk::SurfaceKHR& surface) const;

	public:
		void waitIdle() const;
		
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

		[[nodiscard]] utils::StagingResource<vk::ImageView> createSwapChainViews(
			const vk::SwapchainKHR& swapChain,
			vk::Format format
		) const;

		[[nodiscard]] vk::UniqueImageView createImageView(
			const vk::Image& image,
			vk::Format format,
			vk::ImageAspectFlags aspectFlags
		) const;

		[[nodiscard]] vk::UniqueFramebuffer createFramebuffer(
			const vk::RenderPass& pass,
			const vk::Extent2D& extent,
			const std::vector<vk::ImageView>& images
		) const;

		[[nodiscard]] utils::StagingResource<vk::Framebuffer> createStagedFramebuffer(
			const vk::RenderPass& pass,
			const vk::Extent2D& extent,
			const utils::StagingResource<vk::ImageView>& images
		) const;

		[[nodiscard]] vk::UniqueRenderPass createSimpleRenderPass(
			const std::vector<RenderPassAttachment>& colorAttachments
		) const;
		
		[[nodiscard]] utils::StagingResource<vk::CommandBuffer> createCommandBuffer(
			uint32_t stages,
			vk::CommandBufferLevel level
		) const;

		[[nodiscard]] utils::StagingResource<vk::Semaphore> createSemaphore(
			uint32_t stages
		) const;
		
		[[nodiscard]] utils::StagingResource<vk::Fence> createFence(
			uint32_t stages,
			bool signaled
		) const;
		
		void wait(const vk::Fence& fence) const;
		void reset(const vk::Fence& fence) const;
		
		vk::ResultValue<uint32_t> acquireNextImage(
			const vk::SwapchainKHR& swapChain,
			const vk::Semaphore& semaphore
		) const;

		void submit(
			const vk::CommandBuffer& commandBuffer,
			const vk::Semaphore& waitSemaphore,
			const vk::Semaphore& signalSemaphore,
			const vk::Fence& fence
		) const;
		
		[[nodiscard]] vk::Result present(
			const vk::Semaphore& waitSemaphore,
			const vk::SwapchainKHR& swapChain,
			uint32_t imageIndex
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
