#include "vk_image.h"

#include "vk_context.h"

namespace digbuild::platform::desktop::vulkan
{
	VulkanImage::VulkanImage(
		std::shared_ptr<VulkanContext> context,
		vk::UniqueImage image,
	    const vma::Allocation memoryAllocation
	):
		m_context(std::move(context)),
		m_image(std::move(image)),
		m_memoryAllocation(memoryAllocation)
	{
	}

	VulkanImage::~VulkanImage()
	{
		m_context->m_memoryAllocator.freeMemory(m_memoryAllocation);
	}
}
