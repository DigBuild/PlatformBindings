#include "vk_uniform_buffer.h"

namespace digbuild::platform::desktop::vulkan
{
	UniformBuffer::UniformBuffer(
		std::shared_ptr<VulkanContext> context,
		std::shared_ptr<Shader> shader,
		const uint32_t stages
	) :
		m_context(std::move(context)),
		m_shader(std::move(shader))
	{
		m_descriptorPool = context->createDescriptorPool(stages);
		m_descriptorSets = context->createDescriptorSets(
			*m_descriptorPool,
			m_shader->getDescriptorSetLayout(),
			stages
		);
		
		const auto bindingCount = m_shader->getBindings().size();
		m_buffers.resize(stages);
		for (auto i = 0u; i < stages; ++i)
			m_buffers[i].resize(bindingCount);
	}

	void UniformBuffer::tick()
	{
		const auto writeIndex = (m_readIndex + 1) % static_cast<uint32_t>(m_buffers.size());

		if (m_leftoverWrites == 0)
		{
			m_readIndex = writeIndex;
			return;
		}
		
		auto& bufs = m_buffers[writeIndex];
		std::vector<vk::WriteDescriptorSet> writes;
		writes.reserve(bufs.size());

		auto i = 0u;
		for (const auto& binding : m_shader->getBindings())
		{
			auto& buf = bufs[i];
			
			if (!buf || buf->size() < m_uniformData.size())
			{
				buf = m_context->createBuffer(
					static_cast<uint32_t>(m_uniformData.size()),
					vk::BufferUsageFlagBits::eUniformBuffer,
					vk::SharingMode::eExclusive,
					vk::MemoryPropertyFlagBits::eHostVisible | vk::MemoryPropertyFlagBits::eHostCoherent
				);
			}

			void* memory = buf->mapMemory();
			memcpy(memory, m_uniformData.data(), m_uniformData.size());
			buf->unmapMemory();

			vk::WriteDescriptorSet write{
				*m_descriptorSets[writeIndex],
				i,
				0,
				vk::DescriptorType::eUniformBuffer,
				{},
				std::vector{
					vk::DescriptorBufferInfo{ buf->buffer(), 0, binding.size }
				},
				{}
			};

			i++;
		}

		m_context->updateDescriptorSets(writes, {});

		m_leftoverWrites--;
		m_readIndex = writeIndex;
	}

	void UniformBuffer::write(const std::vector<uint8_t>& data)
	{
		m_uniformData.assign(data.begin(), data.end());
		m_leftoverWrites = static_cast<uint32_t>(m_buffers.size());
	}
}
