#include "vk_texture_sampler.h"

namespace digbuild::platform::desktop::vulkan
{
	vk::SamplerAddressMode toVulkan(const render::TextureWrapping wrapping)
	{
		switch (wrapping)
		{
		case render::TextureWrapping::REPEAT:
			return vk::SamplerAddressMode::eRepeat;
		case render::TextureWrapping::MIRRORED_REPEAT:
			return vk::SamplerAddressMode::eMirroredRepeat;
		case render::TextureWrapping::CLAMP_TO_EDGE:
			return vk::SamplerAddressMode::eClampToEdge;
		case render::TextureWrapping::MIRRORED_CLAMP_TO_EDGE:
			return vk::SamplerAddressMode::eMirrorClampToEdge;
		case render::TextureWrapping::CLAMP_TO_BORDER:
			return vk::SamplerAddressMode::eClampToBorder;
		}
		throw std::runtime_error("Invalid type.");
	}

	vk::Filter toVulkan(const render::TextureFiltering filtering)
	{
		switch (filtering)
		{
		case render::TextureFiltering::LINEAR:
			return vk::Filter::eLinear;
		case render::TextureFiltering::NEAREST:
			return vk::Filter::eNearest;
		}
		throw std::runtime_error("Invalid type.");
	}

	vk::BorderColor toVulkan(const render::TextureBorderColor color)
	{
		switch (color)
		{
		case render::TextureBorderColor::TRANSPARENT_BLACK:
			return vk::BorderColor::eIntTransparentBlack;
		case render::TextureBorderColor::OPAQUE_BLACK:
			return vk::BorderColor::eIntOpaqueBlack;
		case render::TextureBorderColor::OPAQUE_WHITE:
			return vk::BorderColor::eIntOpaqueWhite;
		}
		throw std::runtime_error("Invalid type.");
	}
	
	TextureSampler::TextureSampler(
		std::shared_ptr<VulkanContext> context,
		const render::TextureFiltering minFiltering,
		const render::TextureFiltering magFiltering,
		const render::TextureWrapping wrapping,
		const render::TextureBorderColor borderColor,
		const bool enableAnisotropy,
		const uint32_t anisotropyLevel
	) :
		m_context(std::move(context))
	{
		const auto addressMode = toVulkan(wrapping);
		m_sampler = m_context->createTextureSampler(
			toVulkan(minFiltering),
			toVulkan(magFiltering),
			addressMode,
			vk::SamplerMipmapMode::eNearest, 0.0f, 0.0f, 0.0f,
			enableAnisotropy, static_cast<float>(anisotropyLevel),
			false, vk::CompareOp::eAlways,
			toVulkan(borderColor),
			false
		);
	}
}
