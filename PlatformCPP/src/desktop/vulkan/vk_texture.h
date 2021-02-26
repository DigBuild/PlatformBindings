#pragma once
#include "vk_context.h"
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
		);

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
