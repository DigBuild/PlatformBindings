﻿#include "vk_buffer.h"

#include "vk_context.h"

namespace digbuild::platform::desktop::vulkan
{
	VulkanBuffer::VulkanBuffer(
		std::shared_ptr<VulkanContext> context,
		vk::UniqueBuffer buffer,
		vma::Allocation memoryAllocation,
		const uint32_t size
	) :
		m_context(std::move(context)),
		m_buffer(std::move(buffer)),
		m_memoryAllocation(memoryAllocation),
		m_size(size)//,
		// m_mappedMemory(false)
	{
	}

	VulkanBuffer::~VulkanBuffer()
	{
		m_context->m_memoryAllocator.freeMemory(m_memoryAllocation);
	}

	// void* VulkanBuffer::mapMemory()
	// {
	// 	if (m_mappedMemory)
	// 		throw std::runtime_error("Memory is already mapped.");
	// 	m_mappedMemory = true;
	// 	return m_context->m_device->mapMemory(*m_memory, 0, m_size, {});
	// }
	//
	// void VulkanBuffer::unmapMemory()
	// {
	// 	if (!m_mappedMemory)
	// 		throw std::runtime_error("Memory is not mapped.");
	// 	m_context->m_device->unmapMemory(*m_memory);
	// 	m_mappedMemory = false;
	// }
}
