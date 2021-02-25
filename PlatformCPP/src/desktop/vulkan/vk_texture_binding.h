#pragma once
#include "vk_context.h"
#include "vk_shader.h"
#include "vk_texture.h"
#include "vk_texture_sampler.h"
#include "../../render/texture_binding.h"

namespace digbuild::platform::desktop::vulkan
{
	class TextureBinding final : public render::TextureBinding
	{
	public:
		TextureBinding(
			std::shared_ptr<VulkanContext> context,
			std::shared_ptr<Shader> shader,
			uint32_t binding,
			uint32_t stages,
			const std::shared_ptr<render::TextureSampler>& sampler,
			const std::shared_ptr<render::Texture>& texture
		);

		void tick();
		
		void update(
			std::shared_ptr<render::TextureSampler> sampler,
			std::shared_ptr<render::Texture> texture
		) override;

		[[nodiscard]] vk::DescriptorSet& get()
		{
			return *m_descriptorSets[m_readIndex];
		}

		[[nodiscard]] std::shared_ptr<Shader>& getShader()
		{
			return m_shader;
		}

		[[nodiscard]] uint32_t getBinding() const
		{
			return m_binding;
		}
	
	private:
		std::shared_ptr<VulkanContext> m_context;
		std::shared_ptr<Shader> m_shader;
		uint32_t m_binding;

		std::vector<std::shared_ptr<TextureSampler>> m_samplers;
		std::vector<std::shared_ptr<Texture>> m_textures;

		vk::UniqueDescriptorPool m_descriptorPool;
		std::vector<vk::UniqueDescriptorSet> m_descriptorSets;
		
		uint32_t m_readIndex = 0;
		uint32_t m_leftoverWrites = 0;
	};
}
