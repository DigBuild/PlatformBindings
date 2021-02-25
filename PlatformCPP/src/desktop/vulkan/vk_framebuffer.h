#pragma once
#include "vk_framebuffer_format.h"
#include "vk_texture.h"
#include "../../render/framebuffer.h"

namespace digbuild::platform::desktop::vulkan
{
	class FramebufferTexture final : public Texture {
	public:
		FramebufferTexture(
			std::shared_ptr<VulkanContext> context,
			std::vector<std::unique_ptr<VulkanImage>> images,
			std::vector<vk::UniqueImageView> imageViews,
			const render::TextureFormat format,
			const uint32_t width,
			const uint32_t height
		) :
			m_context(std::move(context)),
			m_images(std::move(images)),
			m_imageViews(std::move(imageViews)),
			m_format(format),
			m_width(width),
			m_height(height)
		{
		}
		
		[[nodiscard]] render::TextureFormat getFormat() override
		{
			return m_format;
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
			return *m_imageViews[m_readIndex];
		}

	private:
		std::shared_ptr<VulkanContext> m_context;
		std::vector<std::unique_ptr<VulkanImage>> m_images;
		std::vector<vk::UniqueImageView> m_imageViews;
		render::TextureFormat m_format;
		uint32_t m_width, m_height;
		uint32_t m_readIndex = 0;

		friend class Framebuffer;
	};
	
	class Framebuffer final : public render::Framebuffer
	{
	public:
		Framebuffer(
			std::shared_ptr<VulkanContext> context,
			std::shared_ptr<FramebufferFormat> format,
			uint32_t width, uint32_t height,
			uint32_t stages
		);
		
		Framebuffer(
			std::shared_ptr<VulkanContext> context,
			std::shared_ptr<FramebufferFormat> format,
			uint32_t width, uint32_t height,
			std::vector<vk::UniqueImageView> imageViews,
			std::vector<vk::UniqueFramebuffer> framebuffers
		);

		[[nodiscard]] const FramebufferFormat& getFormat() const override
		{
			return *m_format;
		}
		[[nodiscard]] uint32_t getWidth() const override
		{
			return m_width;
		}
		[[nodiscard]] uint32_t getHeight() const override
		{
			return m_height;
		}
		[[nodiscard]] std::shared_ptr<render::Texture> getTexture(const uint32_t attachment) override
		{
			return m_textures[attachment];
		}

		[[nodiscard]] vk::Framebuffer& getTarget()
		{
			return *m_framebuffers[getWriteIndex()];
		}

		void advance()
		{
			m_readIndex = getWriteIndex();
			for (auto& texture : m_textures)
				texture->m_readIndex = m_readIndex;
		}

		void transitionTexturesPost(const vk::CommandBuffer& cmd);

	private:
		[[nodiscard]] uint32_t getWriteIndex() const
		{
			return (m_readIndex + 1) % m_framebuffers.size();
		}
		
		std::shared_ptr<VulkanContext> m_context;
		
		const std::shared_ptr<FramebufferFormat> m_format;
		const uint32_t m_width, m_height;

		std::vector<vk::UniqueFramebuffer> m_framebuffers;
		std::vector<std::shared_ptr<FramebufferTexture>> m_textures;
		uint32_t m_readIndex = 0;
		bool m_shouldTransition;
	};
}
