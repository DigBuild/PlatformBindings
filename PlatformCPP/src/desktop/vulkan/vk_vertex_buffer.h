#pragma once
#include "vk_buffer.h"
#include "vk_context.h"
#include "../../render/vertex_buffer.h"

namespace digbuild::platform::desktop::vulkan
{
	class VertexBuffer : public render::VertexBuffer
	{
	public:
		[[nodiscard]] virtual vk::Buffer& get() = 0;
		[[nodiscard]] virtual uint32_t size() = 0;
	};

	class StaticVertexBuffer final : public VertexBuffer
	{
	public:
		StaticVertexBuffer(
			std::shared_ptr<VulkanContext> context,
			const std::vector<uint8_t>& data,
			uint32_t vertexSize
		);

		[[nodiscard]] uint32_t getVertexSize() override
		{
			return m_vertexSize;
		}

		void write(const std::vector<uint8_t>& data) override
		{
			throw std::runtime_error("Cannot write to a static vertex buffer.");
		}

		[[nodiscard]] vk::Buffer& get() override
		{
			return m_buffer->buffer();
		}

		[[nodiscard]] uint32_t size() override
		{
			return m_size;
		}

	private:
		std::shared_ptr<VulkanContext> m_context;
		std::unique_ptr<VulkanBuffer> m_buffer;
		uint32_t m_vertexSize, m_size;
	};

	class DynamicVertexBuffer final : public VertexBuffer
	{
	public:
		DynamicVertexBuffer(
			std::shared_ptr<VulkanContext> context,
			const std::vector<uint8_t>& data,
			uint32_t vertexSize,
			uint32_t stages
		);

		void tick();

		[[nodiscard]] uint32_t getVertexSize() override
		{
			return m_vertexSize;
		}

		void write(const std::vector<uint8_t>& data) override;

		[[nodiscard]] vk::Buffer& get() override;

		[[nodiscard]] uint32_t size() override;

	private:
		void advanceIfNeeded();
		uint32_t getWriteIndex() const;

		std::shared_ptr<VulkanContext> m_context;
		std::vector<std::unique_ptr<VulkanBuffer>> m_buffers;
		std::vector<uint32_t> m_sizes;
		uint32_t m_vertexSize;
		uint32_t m_readIndex = 0;
		bool m_advanceIndex = false;
	};
}
