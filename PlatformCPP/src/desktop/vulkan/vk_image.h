#pragma once
#include <memory>
#include <vulkan.h>

namespace digbuild::platform::desktop::vulkan
{
	class VulkanContext;
	
	class VulkanImage final
	{
	public:
		VulkanImage(
			std::shared_ptr<VulkanContext> context,
			vk::UniqueImage image,
			vk::UniqueDeviceMemory memory
		) :
			m_context(std::move(context)),
			m_image(std::move(image)),
			m_memory(std::move(memory))
		{
		}
		~VulkanImage() = default;
		VulkanImage(const VulkanImage& other) = delete;
		VulkanImage(VulkanImage&& other) noexcept = delete;
		VulkanImage& operator=(const VulkanImage& other) = delete;
		VulkanImage& operator=(VulkanImage&& other) noexcept = delete;

		[[nodiscard]] vk::Image& get()
		{
			return *m_image;
		}
	
	private:
		std::shared_ptr<VulkanContext> m_context;
		vk::UniqueImage m_image;
		vk::UniqueDeviceMemory m_memory;
	};
}
