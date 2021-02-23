#pragma once
#include "vk_framebuffer_format.h"
#include "../../render/framebuffer.h"

namespace digbuild::platform::desktop::vulkan
{
	class Framebuffer final : public render::Framebuffer
	{
	public:
		Framebuffer(
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
			m_imageViews(std::move(imageViews)),
			m_framebuffers(std::move(framebuffers))
		{
		}

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

		[[nodiscard]] vk::Framebuffer& get()
		{
			return *m_framebuffers[m_readIndex];
		}

		void next()
		{
			m_readIndex = (m_readIndex + 1) % m_framebuffers.size();
		}
		
		[[nodiscard]] vk::Framebuffer& operator[](size_t i)
		{
			return *m_framebuffers[i];
		}

	private:
		std::shared_ptr<VulkanContext> m_context;
		
		const std::shared_ptr<FramebufferFormat> m_format;
		const uint32_t m_width, m_height;
		
		std::vector<vk::UniqueImageView> m_imageViews;
		std::vector<vk::UniqueFramebuffer> m_framebuffers;
		uint32_t m_readIndex = 0;
	};
}
