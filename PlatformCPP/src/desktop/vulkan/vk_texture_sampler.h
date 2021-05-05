#pragma once
#include "vk_context.h"
#include "../../render/render_context.h"

namespace digbuild::platform::desktop::vulkan
{
	class TextureSampler final : public render::TextureSampler
	{
	public:
		TextureSampler(
			std::shared_ptr<VulkanContext> context,
			render::TextureFiltering minFiltering,
			render::TextureFiltering magFiltering,
			render::TextureWrapping wrapping,
			render::TextureBorderColor borderColor,
			bool enableAnisotropy,
			uint32_t anisotropyLevel
		);

		[[nodiscard]] const vk::Sampler& get() const
		{
			return *m_sampler;
		}
	
	private:
		std::shared_ptr<VulkanContext> m_context;
		vk::UniqueSampler m_sampler;
	};
}
