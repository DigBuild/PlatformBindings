#pragma once
#include "vk_context.h"
#include "vk_shader.h"
#include "../../render/uniform_buffer.h"

namespace digbuild::platform::desktop::vulkan
{
	class UniformBuffer final : public render::UniformBuffer
	{
	public:
		UniformBuffer(
			std::shared_ptr<VulkanContext> context,
			uint32_t stages,
			const std::vector<uint8_t>& initialData
		);

		void tick();
		
		void write(const std::vector<uint8_t>& data) override;
		

		[[nodiscard]] vk::Buffer& getBuffer() const
		{
			return m_buffers[m_readIndex]->buffer();
		}

	private:
		std::shared_ptr<VulkanContext> m_context;
		
		std::vector<std::unique_ptr<VulkanBuffer>> m_buffers;
		std::vector<uint8_t> m_uniformData;

		uint32_t m_readIndex = 0;
		uint32_t m_leftoverWrites = 0;
	};
}
