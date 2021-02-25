#include "vk_vertex_buffer.h"

namespace digbuild::platform::desktop::vulkan
{
	StaticVertexBuffer::StaticVertexBuffer(
		std::shared_ptr<VulkanContext> context,
		const std::vector<uint8_t>& data,
		const uint32_t vertexSize
	) :
		m_context(std::move(context)),
		m_vertexSize(vertexSize)
	{
		m_buffer = m_context->createBuffer(
			static_cast<uint32_t>(data.size()),
			vk::BufferUsageFlagBits::eVertexBuffer,
			vk::SharingMode::eExclusive,
			vk::MemoryPropertyFlagBits::eHostVisible | vk::MemoryPropertyFlagBits::eHostCoherent
		);
		
		auto* memory = m_buffer->mapMemory();
		memcpy(memory, data.data(), data.size());
		m_buffer->unmapMemory();
		
		m_size = static_cast<uint32_t>(data.size() / vertexSize);
	}

	DynamicVertexBuffer::DynamicVertexBuffer(
		std::shared_ptr<VulkanContext> context,
		const std::vector<uint8_t>& data,
		const uint32_t vertexSize,
		const uint32_t stages
	) :
		m_context(std::move(context)),
		m_vertexSize(vertexSize)
	{
		m_buffers.resize(stages);
		m_sizes.resize(stages);

		if (!data.empty())
			write(data);
	}

	void DynamicVertexBuffer::tick()
	{
		advanceIfNeeded();
	}

	void DynamicVertexBuffer::write(const std::vector<uint8_t>& data)
	{
		const auto writeIndex = getWriteIndex();
		auto& buf = m_buffers[writeIndex];

		if (!buf || buf->size() < data.size())
		{
			buf = m_context->createBuffer(
				static_cast<uint32_t>(data.size()),
				vk::BufferUsageFlagBits::eVertexBuffer,
				vk::SharingMode::eExclusive,
				vk::MemoryPropertyFlagBits::eHostVisible | vk::MemoryPropertyFlagBits::eHostCoherent
			);
		}
		
		auto* memory = buf->mapMemory();
		memcpy(memory, data.data(), data.size());
		buf->unmapMemory();

		m_sizes[writeIndex] = static_cast<uint32_t>(data.size() / m_vertexSize);

		m_advanceIndex = true;
	}
	
	vk::Buffer& DynamicVertexBuffer::get()
	{
		advanceIfNeeded();
		return m_buffers[m_readIndex]->buffer();
	}

	uint32_t DynamicVertexBuffer::size()
	{
		advanceIfNeeded();
		return m_sizes[m_readIndex];
	}

	void DynamicVertexBuffer::advanceIfNeeded()
	{
		if (m_advanceIndex)
		{
			m_advanceIndex = false;
			m_readIndex = getWriteIndex();
		}
	}

	uint32_t DynamicVertexBuffer::getWriteIndex() const
	{
		return (m_readIndex + 1) % m_buffers.size();
	}
}
