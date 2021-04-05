#include "vk_framebuffer.h"

namespace digbuild::platform::desktop::vulkan
{
	vk::ImageAspectFlags toVulkanAspectFlags(const render::FramebufferAttachmentType type)
	{
		switch (type)
		{
		case render::FramebufferAttachmentType::COLOR:
			return vk::ImageAspectFlagBits::eColor;
		case render::FramebufferAttachmentType::DEPTH_STENCIL:
			return vk::ImageAspectFlagBits::eDepth | vk::ImageAspectFlagBits::eStencil;
		}
		throw std::runtime_error("Invalid type.");
	}
	
	vk::ImageUsageFlagBits toVulkanUsageFlags(const render::FramebufferAttachmentType type)
	{
		switch (type)
		{
		case render::FramebufferAttachmentType::COLOR:
			return vk::ImageUsageFlagBits::eColorAttachment;
		case render::FramebufferAttachmentType::DEPTH_STENCIL:
			return vk::ImageUsageFlagBits::eDepthStencilAttachment;
		}
		throw std::runtime_error("Invalid type.");
	}

	Framebuffer::Framebuffer(
		std::shared_ptr<VulkanContext> context,
		std::shared_ptr<FramebufferFormat> format,
		const uint32_t width, const uint32_t height,
		const uint32_t stages
	) :
		m_context(std::move(context)),
		m_format(std::move(format)),
		m_width(width),
		m_height(height),
		m_shouldTransition(true)
	{
		const auto& attachments = m_format->getAttachments();
		m_textures.reserve(attachments.size());

		std::vector<std::vector<vk::ImageView>> framebufferViews;
		framebufferViews.resize(stages);

		for (const auto& attachment : attachments)
		{
			const auto usageFlags = toVulkanUsageFlags(attachment.type) | vk::ImageUsageFlagBits::eSampled;
			const auto vkFormat = util::toVulkanFormat(attachment.format);
			const auto aspectFlags = toVulkanAspectFlags(attachment.type);

			std::vector<std::unique_ptr<VulkanImage>> images;
			std::vector<vk::UniqueImageView> views;
			images.reserve(stages);
			views.reserve(stages);
			for (auto i = 0u; i < stages; ++i)
			{
				auto image = m_context->createImage(width, height, vkFormat, usageFlags, vk::MemoryPropertyFlagBits::eDeviceLocal);
				auto view = m_context->createImageView(image->get(), vkFormat, aspectFlags);
				
				framebufferViews[i].push_back(*view);

				util::transitionImageLayoutsImmediate(*m_context->m_device, *m_context->m_commandPool, m_context->m_graphicsQueue, {{
					image->get(),
					aspectFlags,
					vk::ImageLayout::eUndefined,
					attachment.type == render::FramebufferAttachmentType::COLOR ?
						vk::ImageLayout::eColorAttachmentOptimal :
						vk::ImageLayout::eDepthStencilAttachmentOptimal
				}});
				
				images.push_back(std::move(image));
				views.push_back(std::move(view));
			}
			
			m_textures.push_back(std::make_shared<FramebufferTexture>(
				m_context,
				std::move(images),
				std::move(views),
				width, height
			));
		}
		
		m_framebuffers = m_context->createFramebuffers(m_format->getPass(), { width, height }, framebufferViews);
	}

	Framebuffer::Framebuffer(
		std::shared_ptr<VulkanContext> context,
		std::shared_ptr<FramebufferFormat> format,
		const uint32_t width, const uint32_t height,
		std::vector<vk::UniqueImageView> imageViews,
		std::vector<vk::UniqueFramebuffer> framebuffers
	) :
		m_context(std::move(context)),
		m_format(std::move(format)),
		m_width(width),
		m_height(height),
		m_framebuffers(std::move(framebuffers)),
		m_writeIndex(static_cast<uint32_t>(imageViews.size() - 1)),
		m_shouldTransition(false)
	{
		m_textures.push_back(std::make_shared<FramebufferTexture>(
			m_context,
			std::vector<std::unique_ptr<VulkanImage>>{},
			std::move(imageViews),
			width, height
		));
	}

	void Framebuffer::advance()
	{
		m_writeIndex = getWriteIndex();
		for (auto& texture : m_textures)
			texture->m_readIndex = (m_writeIndex + 1) % m_framebuffers.size();
	}

	void Framebuffer::transitionTexturesPost(const vk::CommandBuffer& cmd)
	{
		if (!m_shouldTransition)
			return;

		const auto& attachments = m_format->getAttachments();

		auto i = 0u;
		for (auto& texture : m_textures)
		{
			const auto& attachment = attachments[i];
			util::transitionImageLayouts(cmd, {{
				texture->m_images[texture->m_readIndex]->get(),
				toVulkanAspectFlags(attachment.type),
				attachment.type == render::FramebufferAttachmentType::COLOR ?
					vk::ImageLayout::eColorAttachmentOptimal :
					vk::ImageLayout::eDepthStencilAttachmentOptimal,
				vk::ImageLayout::eShaderReadOnlyOptimal
			}});
			i++;
		}
	}
}
