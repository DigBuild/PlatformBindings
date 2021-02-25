#pragma once
#include "vk_util.h"
#include "../../render/texture.h"

namespace digbuild::platform::desktop::vulkan
{
	class Texture : public render::Texture
	{
	public:
		virtual vk::ImageView& get() = 0;
	};

	class StaticTexture : public Texture
	{
	public:
		StaticTexture(
			std::shared_ptr<VulkanContext> context,
			const uint32_t width, const uint32_t height,
			const render::TextureFormat format,
			const std::vector<uint8_t>& data
		) :
			m_context(std::move(context)),
			m_width(width),
			m_height(height)
		{
			const auto fmt = util::toVulkanFormat(format);
			m_image = m_context->createImage(
				width, height,
				fmt,
				vk::ImageUsageFlagBits::eSampled,
				vk::MemoryPropertyFlagBits::eDeviceLocal
			);
			m_imageView = m_context->createImageView(m_image->get(), fmt, vk::ImageAspectFlagBits::eColor);

			auto buf = m_context->createBuffer(
				static_cast<uint32_t>(data.size()),
				vk::BufferUsageFlagBits::eTransferSrc,
				vk::SharingMode::eExclusive,
				vk::MemoryPropertyFlagBits::eHostVisible | vk::MemoryPropertyFlagBits::eHostCoherent
			);
			auto* mem = buf->mapMemory();
			memcpy(mem, data.data(), data.size());
			buf->unmapMemory();

			util::directExecuteCommands(
				*m_context->m_device,
				*m_context->m_commandPool,
				m_context->m_graphicsQueue,
				[&](vk::CommandBuffer& cmd) {
					util::transitionImageLayouts(cmd, {{
						m_image->get(),
						vk::ImageAspectFlagBits::eColor,
						vk::ImageLayout::eUndefined,
						vk::ImageLayout::eTransferDstOptimal
					}});
					util::copyBufferToImage(cmd, buf->buffer(), m_image->get(), width, height);
					util::transitionImageLayouts(cmd, {{
						m_image->get(),
						vk::ImageAspectFlagBits::eColor,
						vk::ImageLayout::eTransferDstOptimal,
						vk::ImageLayout::eShaderReadOnlyOptimal
					}});
				}
			);
		}

		[[nodiscard]] uint32_t getWidth() override
		{
			return m_width;
		}
		[[nodiscard]] uint32_t getHeight() override
		{
			return m_height;
		}
		
		[[nodiscard]] vk::ImageView& get() override
		{
			return *m_imageView;
		}

	private:
		std::shared_ptr<VulkanContext> m_context;
		uint32_t m_width, m_height;
		std::unique_ptr<VulkanImage> m_image;
		vk::UniqueImageView m_imageView;
	};
}
