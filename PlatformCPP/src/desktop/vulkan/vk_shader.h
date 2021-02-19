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

		[[nodiscard]] std::vector<render::ShaderBinding>& getBindings()
		{
			return m_bindings;
		}

		[[nodiscard]] vk::ShaderStageFlagBits getStage() const;
	
	private:
		std::shared_ptr<VulkanContext> m_context;
		vk::UniqueShaderModule m_module;
		render::ShaderType m_type;
		std::vector<render::ShaderBinding> m_bindings;
	};
}
