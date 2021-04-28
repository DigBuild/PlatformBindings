#include "vk_texture.h"

namespace digbuild::platform::desktop::vulkan
{
	StaticTexture::StaticTexture(
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
			vk::ImageUsageFlagBits::eSampled | vk::ImageUsageFlagBits::eTransferDst,
			vk::MemoryPropertyFlagBits::eDeviceLocal
		);
		m_imageView = m_context->createImageView(m_image->get(), fmt, vk::ImageAspectFlagBits::eColor);
		
		auto buf = m_context->createCpuToGpuTransferBuffer(
			data.data(),
			static_cast<uint32_t>(data.size())
		);

		util::directExecuteCommands(
			*m_context->m_device,
			*m_context->m_commandPool,
			m_context->m_graphicsQueue,
			[&](vk::CommandBuffer& cmd)
			{
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
}
