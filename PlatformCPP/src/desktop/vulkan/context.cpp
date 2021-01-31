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

	VulkanContext::~VulkanContext()
	{
		waitIdle();
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

	void VulkanContext::waitIdle() const
	{
		if (m_presentQueue)
			m_presentQueue.waitIdle();
		if (m_graphicsQueue)
			m_graphicsQueue.waitIdle();
		if (m_device)
			m_device->waitIdle();
	}

	[[nodiscard]] utils::SwapChainDescriptor VulkanContext::getSwapChainDescriptor(const vk::SurfaceKHR& surface) const
	{
		return utils::getSwapChainDescriptor(m_physicalDevice, surface);
	}

	[[nodiscard]] vk::UniqueSwapchainKHR VulkanContext::createSwapChain(
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

	[[nodiscard]] utils::StagingResource<vk::ImageView> VulkanContext::createSwapChainViews(
		const vk::SwapchainKHR& swapChain,
		const vk::Format format
	) const
	{
		const auto images = m_device->getSwapchainImagesKHR(swapChain);
		std::vector<vk::UniqueImageView> views;
		views.reserve(images.size());
		for (const auto& image : images)
			views.push_back(createImageView(image, format, vk::ImageAspectFlagBits::eColor));
		return utils::StagingResource<vk::ImageView>(std::move(views));
	}

	[[nodiscard]] vk::UniqueImageView VulkanContext::createImageView(
		const vk::Image& image,
		const vk::Format format,
		const vk::ImageAspectFlags aspectFlags
	) const
	{
		return m_device->createImageViewUnique({
			{},
			image,
			vk::ImageViewType::e2D,
			format,
			{},
			vk::ImageSubresourceRange{
				aspectFlags,
				0, 1, 0, 1
			}
		});
	}

	[[nodiscard]] vk::UniqueFramebuffer VulkanContext::createFramebuffer(
		const vk::RenderPass& pass,
		const vk::Extent2D& extent,
		const std::vector<vk::ImageView>& images
	) const
	{
		return m_device->createFramebufferUnique({
			{},
			pass,
			static_cast<uint32_t>(images.size()), images.data(),
			extent.width, extent.height, 1
		});
	}

	[[nodiscard]] utils::StagingResource<vk::Framebuffer> VulkanContext::createStagedFramebuffer(
		const vk::RenderPass& pass,
		const vk::Extent2D& extent,
		const utils::StagingResource<vk::ImageView>& images
	) const
	{
		std::vector<vk::UniqueFramebuffer> framebuffers;
		framebuffers.reserve(images.size());
		for (int i = 0; i < images.size(); i++)
			framebuffers.push_back(createFramebuffer(pass, extent, { images[i] }));
		return utils::StagingResource<vk::Framebuffer>(std::move(framebuffers));
	}

	[[nodiscard]] vk::UniqueRenderPass VulkanContext::createSimpleRenderPass(
		const std::vector<RenderPassAttachment>& colorAttachments
	) const
	{
		std::vector<vk::AttachmentDescription> attachments;
		std::vector<vk::AttachmentReference> references;
		attachments.reserve(colorAttachments.size());
		references.reserve(colorAttachments.size());
		for (auto& attachment : colorAttachments) {
			attachments.push_back({
				{},
				attachment.format,
				vk::SampleCountFlagBits::e1,
				vk::AttachmentLoadOp::eClear,
				vk::AttachmentStoreOp::eStore,
				vk::AttachmentLoadOp::eDontCare,
				vk::AttachmentStoreOp::eDontCare,
				vk::ImageLayout::eUndefined,
				attachment.targetLayout
			});
			references.push_back({
				static_cast<uint32_t>(references.size()),
				vk::ImageLayout::eColorAttachmentOptimal
			});
		}
		
		vk::SubpassDescription subpass{
			{},
			vk::PipelineBindPoint::eGraphics,
			0, nullptr,
			static_cast<uint32_t>(references.size()), references.data(),
			nullptr, nullptr,
			0, nullptr
		};
		vk::SubpassDependency subpassDependency{
			VK_SUBPASS_EXTERNAL, 0,
			vk::PipelineStageFlagBits::eColorAttachmentOutput | vk::PipelineStageFlagBits::eEarlyFragmentTests,
			vk::PipelineStageFlagBits::eColorAttachmentOutput | vk::PipelineStageFlagBits::eEarlyFragmentTests,
			{},
			vk::AccessFlagBits::eColorAttachmentWrite
		};
		
		return m_device->createRenderPassUnique({
			{},
			static_cast<uint32_t>(attachments.size()), attachments.data(),
			1, &subpass,
			1, &subpassDependency
		});
	}

	[[nodiscard]] utils::StagingResource<vk::CommandBuffer> VulkanContext::createCommandBuffer(
		const uint32_t stages,
		const vk::CommandBufferLevel level
	) const
	{
		auto commandBuffers = m_device->allocateCommandBuffersUnique({
			*m_commandPool,
			level,
			stages
		});
		return utils::StagingResource<vk::CommandBuffer>(std::move(commandBuffers));
	}

	[[nodiscard]] utils::StagingResource<vk::Semaphore> VulkanContext::createSemaphore(
		const uint32_t stages
	) const
	{
		std::vector<vk::UniqueSemaphore> semaphores;
		semaphores.reserve(stages);
		for (int i = 0; i < stages; ++i)
			semaphores.push_back(m_device->createSemaphoreUnique({}));
		return utils::StagingResource<vk::Semaphore>(std::move(semaphores));
	}

	[[nodiscard]] utils::StagingResource<vk::Fence> VulkanContext::createFence(
		const uint32_t stages,
		const bool signaled
	) const
	{
		vk::FenceCreateInfo createInfo;
		if (signaled)
			createInfo.flags |= vk::FenceCreateFlagBits::eSignaled;
		std::vector<vk::UniqueFence> fences;
		fences.reserve(stages);
		for (int i = 0; i < stages; ++i)
			fences.push_back(m_device->createFenceUnique(createInfo));
		return utils::StagingResource<vk::Fence>(std::move(fences));
	}

	void VulkanContext::wait(const vk::Fence& fence) const
	{
		const auto result = m_device->waitForFences(1, &fence, true, UINT64_MAX);
		if (result != vk::Result::eSuccess)
			throw std::runtime_error("Failed to wait for fence.");
	}

	void VulkanContext::reset(const vk::Fence& fence) const
	{
		const auto result = m_device->resetFences(1, &fence);
		if (result != vk::Result::eSuccess)
			throw std::runtime_error("Failed to reset fence.");
	}

	vk::ResultValue<uint32_t> VulkanContext::acquireNextImage(
		const vk::SwapchainKHR& swapChain,
		const vk::Semaphore& semaphore
	) const
	{
		return m_device->acquireNextImageKHR(swapChain, UINT64_MAX, semaphore, nullptr);
	}

	void VulkanContext::submit(
		const vk::CommandBuffer& commandBuffer,
		const vk::Semaphore& waitSemaphore,
		const vk::Semaphore& signalSemaphore,
		const vk::Fence& fence
	) const
	{
		vk::PipelineStageFlags waitFlags = vk::PipelineStageFlagBits::eColorAttachmentOutput;
		vk::SubmitInfo submitInfo{
			1, &waitSemaphore, &waitFlags,
			1, &commandBuffer,
			1, &signalSemaphore
		};
		const auto result = m_graphicsQueue.submit(1, &submitInfo, fence);
		if (result != vk::Result::eSuccess)
			throw std::runtime_error("Failed to submit work.");
	}

	[[nodiscard]] vk::Result VulkanContext::present(
		const vk::Semaphore& waitSemaphore,
		const vk::SwapchainKHR& swapChain,
		const uint32_t imageIndex
	) const
	{
		vk::PresentInfoKHR presentInfo{
			1, &waitSemaphore,
			1, &swapChain, &imageIndex,
			nullptr
		};
		return m_presentQueue.presentKHR(&presentInfo);
	}
}
