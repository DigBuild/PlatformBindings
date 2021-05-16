#include "vk_uniform_buffer.h"

namespace digbuild::platform::desktop::vulkan
{
	UniformBuffer::UniformBuffer(
		std::shared_ptr<VulkanContext> context,
		const uint32_t stages,
		const std::vector<uint8_t>& initialData
	) :
		m_context(std::move(context))
	{
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
		
		if (!buffer || buffer->size() < m_uniformData.size())
		{
			buffer = m_context->createBuffer(
				static_cast<uint32_t>(m_uniformData.size()),
				vk::BufferUsageFlagBits::eUniformBuffer | vk::BufferUsageFlagBits::eTransferDst,
				vk::SharingMode::eExclusive,
				{}
			);

			for (const auto& dependent : m_dependents)
			{
				const auto binding = dependent.lock();
				if (binding)
					binding->updateNext();
			}
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

	void UniformBuffer::registerUser(const std::weak_ptr<UniformBinding>& binding)
	{
		m_dependents.insert(binding);
	}

	void UniformBuffer::unregisterUser(const UniformBinding* binding)
	{
		const auto iterator = std::find_if(
			m_dependents.begin(), m_dependents.end(),
			[&](const std::weak_ptr<UniformBinding>& ptr) {
				return ptr.lock().get() == binding;
			}
		);
		if (iterator == m_dependents.end())
			return;
		m_dependents.extract(iterator);
	}
}
