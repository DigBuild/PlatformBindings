#include "vk_uniform_buffer.h"

namespace digbuild::platform::desktop::vulkan
{
	UniformBuffer::UniformBuffer(
		std::shared_ptr<VulkanContext> context,
		std::shared_ptr<Shader> shader,
		const uint32_t binding,
		const uint32_t stages,
		const std::vector<uint8_t>& initialData
	) :
		m_context(std::move(context)),
		m_shader(std::move(shader)),
		m_binding(binding)
	{
		m_descriptorPool = m_context->createDescriptorPool(stages);
		m_descriptorSets = m_context->createDescriptorSets(
			*m_descriptorPool,
			m_shader->getDescriptorSetLayouts()[binding],
			stages
		);
		
		const auto bindingCount = m_shader->getBindings().size();
		m_buffers.resize(stages);

		if (!initialData.empty())
			write(initialData);
	}

	void UniformBuffer::tick()
	{
		const auto writeIndex = (m_readIndex + 1) % static_cast<uint32_t>(m_buffers.size());

		if (m_leftoverWrites == 0)
		{
			m_readIndex = writeIndex;
			return;
		}
		
		auto& buf = m_buffers[writeIndex];
		const auto& binding = m_shader->getBindings()[m_binding];
		
		if (!buf || buf->size() < m_uniformData.size())
		{
			buf = m_context->createBuffer(
				static_cast<uint32_t>(m_uniformData.size()),
				vk::BufferUsageFlagBits::eUniformBuffer,
				vk::SharingMode::eExclusive,
				vk::MemoryPropertyFlagBits::eHostVisible | vk::MemoryPropertyFlagBits::eHostCoherent
			);

			const vk::DescriptorBufferInfo bufferInfo{ buf->buffer(), 0, binding.size };
			const vk::WriteDescriptorSet write{
				*m_descriptorSets[writeIndex],
				m_binding,
				0,
				1,
				vk::DescriptorType::eUniformBufferDynamic,
				nullptr,
				&bufferInfo,
				nullptr
			};

			m_context->updateDescriptorSets({ write }, {});
		}

		void* memory = buf->mapMemory();
		memcpy(memory, m_uniformData.data(), m_uniformData.size());
		buf->unmapMemory();

		m_leftoverWrites--;
		m_readIndex = writeIndex;
	}

	void UniformBuffer::write(const std::vector<uint8_t>& data)
	{
		m_uniformData.assign(data.begin(), data.end());
		m_leftoverWrites = static_cast<uint32_t>(m_buffers.size());
	}
}
