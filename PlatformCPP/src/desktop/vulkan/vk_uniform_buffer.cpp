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
		m_descriptorPool = m_context->createDescriptorPool(stages, vk::DescriptorType::eUniformBuffer);
		m_descriptorSets = m_context->createDescriptorSets(
			*m_descriptorPool,
			m_shader->getDescriptorSetLayouts()[binding],
			stages
		);
		
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

		if (m_uniformData.empty())
		{
			m_leftoverWrites--;
			m_readIndex = writeIndex;
			return;
		}
		
		auto& buffer = m_buffers[writeIndex];
		const auto& binding = m_shader->getBindings()[m_binding];
		
		if (!buffer || buffer->size() < m_uniformData.size())
		{
			buffer = m_context->createBuffer(
				static_cast<uint32_t>(m_uniformData.size()),
				vk::BufferUsageFlagBits::eUniformBuffer,
				vk::SharingMode::eExclusive,
				{}
			);

			const vk::DescriptorBufferInfo bufferInfo{ buffer->buffer(), 0, binding.size };
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
		
		auto buf = m_context->createCpuToGpuTransferBuffer(
			m_uniformData.data(),
			static_cast<uint32_t>(m_uniformData.size())
		);

		util::copyBufferToBufferImmediate(
			*m_context->m_device,
			*m_context->m_commandPool,
			m_context->m_graphicsQueue,
			buf->buffer(),
			buffer->buffer(),
			static_cast<uint32_t>(m_uniformData.size())
		);

		m_leftoverWrites--;
		m_readIndex = writeIndex;

		if (m_leftoverWrites == 0)
		{
			m_uniformData.clear();
			m_uniformData.shrink_to_fit();
		}
	}

	void UniformBuffer::write(const std::vector<uint8_t>& data)
	{
		m_uniformData.assign(data.begin(), data.end());
		m_leftoverWrites = static_cast<uint32_t>(m_buffers.size());
	}
}
