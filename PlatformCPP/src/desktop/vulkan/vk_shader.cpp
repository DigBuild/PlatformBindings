#include "vk_shader.h"

namespace digbuild::platform::desktop::vulkan
{
	Shader::Shader(
		std::shared_ptr<VulkanContext> context,
		const render::ShaderType type,
		const std::vector<uint8_t>& data,
		std::vector<render::ShaderBinding> bindings
	) :
		m_context(std::move(context)),
		m_module(m_context->createShaderModule(data)),
		m_type(type),
		m_bindings(std::move(bindings))
	{
	}

	vk::ShaderStageFlagBits Shader::getStage() const
	{
		switch (m_type)
		{
		case render::ShaderType::VERTEX:
			return vk::ShaderStageFlagBits::eVertex;
		case render::ShaderType::FRAGMENT:
			return vk::ShaderStageFlagBits::eFragment;
		}
		throw std::runtime_error("Invalid type.");
	}
}
