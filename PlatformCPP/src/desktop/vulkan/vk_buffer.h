#pragma once
#include <memory>
#include <vulkan.h>

namespace digbuild::platform::desktop::vulkan
{
	class VulkanContext;
	
	class VulkanBuffer final
	{
	public:
		VulkanBuffer(
			std::shared_ptr<VulkanContext> context,
			vk::UniqueBuffer buffer,
			vma::Allocation memoryAllocation,
			uint32_t size
		);
		~VulkanBuffer();
		VulkanBuffer(const VulkanBuffer& other) = delete;
		VulkanBuffer(VulkanBuffer&& other) noexcept = delete;
		VulkanBuffer& operator=(const VulkanBuffer& other) = delete;
		VulkanBuffer& operator=(VulkanBuffer&& other) noexcept = delete;

		[[nodiscard]] uint32_t size() const
		{
			return m_size;
		}
		[[nodiscard]] vk::Buffer& buffer()
		{
			return *m_buffer;
		}

		// [[nodiscard]] void* mapMemory();
		// void unmapMemory();
	
	private:
		std::shared_ptr<VulkanContext> m_context;
		vk::UniqueBuffer m_buffer;
		vma::Allocation m_memoryAllocation;
		uint32_t m_size;
		// bool m_mappedMemory;
	};
}
