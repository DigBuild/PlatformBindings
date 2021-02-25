#include "vk_texture_binding.h"

namespace digbuild::platform::desktop::vulkan
{
	TextureBinding::TextureBinding(
		std::shared_ptr<VulkanContext> context,
		std::shared_ptr<Shader> shader,
		const uint32_t binding,
		const uint32_t stages,
		const std::shared_ptr<render::TextureSampler>& sampler,
		const std::shared_ptr<render::Texture>& texture
	) :
		m_context(std::move(context)),
		m_shader(std::move(shader)),
		m_binding(binding)
	{
		m_samplers.resize(stages);
		m_textures.resize(stages);
		
		m_descriptorPool = m_context->createDescriptorPool(stages, vk::DescriptorType::eCombinedImageSampler);
		m_descriptorSets = m_context->createDescriptorSets(
			*m_descriptorPool,
			m_shader->getDescriptorSetLayouts()[binding],
			stages
		);

		update(sampler, texture);
	}

	void TextureBinding::tick()
	{
		const auto writeIndex = (m_readIndex + 1) % static_cast<uint32_t>(m_descriptorSets.size());

		if (m_leftoverWrites == 0)
		{
			m_readIndex = writeIndex;
			return;
		}

		const vk::DescriptorImageInfo imageInfo{
			m_samplers[writeIndex]->get(),
			m_textures[writeIndex]->get(),
			vk::ImageLayout::eShaderReadOnlyOptimal
		};
		const vk::WriteDescriptorSet write{
			*m_descriptorSets[writeIndex],
			m_binding,
			0,
			1,
			vk::DescriptorType::eCombinedImageSampler,
			&imageInfo,
			nullptr,
			nullptr
		};

		m_context->updateDescriptorSets({ write }, {});

		const auto nextWriteIndex = (writeIndex + 1) % static_cast<uint32_t>(m_descriptorSets.size());
		m_samplers[nextWriteIndex] = m_samplers[writeIndex];
		m_textures[nextWriteIndex] = m_textures[writeIndex];

		m_leftoverWrites--;
		m_readIndex = writeIndex;
	}

	void TextureBinding::update(
		const std::shared_ptr<render::TextureSampler> sampler,
		const std::shared_ptr<render::Texture> texture
	)
	{
		const auto writeIndex = (m_readIndex + 1) % static_cast<uint32_t>(m_descriptorSets.size());
		m_samplers[writeIndex] = std::static_pointer_cast<TextureSampler>(sampler);
		m_textures[writeIndex] = std::static_pointer_cast<Texture>(texture);

		m_leftoverWrites = static_cast<uint32_t>(m_descriptorSets.size());
	}
}
