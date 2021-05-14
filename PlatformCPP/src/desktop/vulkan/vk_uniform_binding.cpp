#include "vk_uniform_binding.h"
#include "vk_uniform_buffer.h"

namespace digbuild::platform::desktop::vulkan
{
	UniformBinding::UniformBinding(
		std::shared_ptr<VulkanContext> context,
		std::shared_ptr<Shader> shader,
		const uint32_t binding,
		const uint32_t stages,
		const std::shared_ptr<render::UniformBuffer>& uniformBuffer
	) :
		m_context(std::move(context)),
		m_shader(std::move(shader)),
		m_binding(binding),
		m_bindingSize(m_shader->getBindings()[binding].size)
	{
		m_buffers.resize(stages);
		
		m_descriptorPool = m_context->createDescriptorPool(stages, vk::DescriptorType::eUniformBuffer);
		m_descriptorSets = m_context->createDescriptorSets(
			*m_descriptorPool,
			m_shader->getDescriptorSetLayouts()[binding],
			stages
		);

		if (uniformBuffer != nullptr)
			update(uniformBuffer);
	}

	void UniformBinding::tick()
	{
		const auto writeIndex = (m_readIndex + 1) % static_cast<uint32_t>(m_descriptorSets.size());

		if (m_leftoverWrites == 0)
		{
			m_readIndex = writeIndex;
			return;
		}
		
		const vk::DescriptorBufferInfo bufferInfo{ m_buffers[writeIndex]->buffer(), 0, m_bindingSize };
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

		const auto nextWriteIndex = (writeIndex + 1) % static_cast<uint32_t>(m_descriptorSets.size());
		m_buffers[nextWriteIndex] = m_buffers[writeIndex];

		m_leftoverWrites--;
		m_readIndex = writeIndex;
	}

	void UniformBinding::update(
		const std::shared_ptr<render::UniformBuffer> uniformBuffer
	)
	{
		const auto writeIndex = (m_readIndex + 1) % static_cast<uint32_t>(m_descriptorSets.size());
		m_buffers[writeIndex] = std::static_pointer_cast<UniformBuffer>(uniformBuffer);

		m_leftoverWrites = static_cast<uint32_t>(m_descriptorSets.size());
	}
}
