#pragma once
#include "vk_context.h"
#include "../../render/render_context.h"
#include "../../render/shader.h"

namespace digbuild::platform::desktop::vulkan
{
	class Shader final : public render::Shader
	{
	public:
		Shader(
			std::shared_ptr<VulkanContext> context,
			render::ShaderType type,
			const std::vector<uint8_t>& data,
			std::vector<render::ShaderBinding> bindings
		);

		[[nodiscard]] vk::ShaderModule& getModule()
		{
			return *m_module;
		}

		[[nodiscard]] const std::vector<render::ShaderBinding>& getBindings() const
		{
			return m_bindings;
		}

		[[nodiscard]] vk::DescriptorSetLayout& getDescriptorSetLayout()
		{
			return *m_layoutDesc;
		}

		[[nodiscard]] vk::ShaderStageFlagBits getStage() const
		{
			return m_stage;
		}

	private:
		std::shared_ptr<VulkanContext> m_context;
		vk::UniqueShaderModule m_module;
		std::vector<render::ShaderBinding> m_bindings;
		vk::UniqueDescriptorSetLayout m_layoutDesc;
		vk::ShaderStageFlagBits m_stage;
	};
}
