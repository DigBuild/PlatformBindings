#include "vk_shader.h"

namespace digbuild::platform::desktop::vulkan
{
	vk::ShaderStageFlagBits toVulkan(const render::ShaderType type)
	{
		switch (type)
		{
		case render::ShaderType::VERTEX:
			return vk::ShaderStageFlagBits::eVertex;
		case render::ShaderType::FRAGMENT:
			return vk::ShaderStageFlagBits::eFragment;
		}
		throw std::runtime_error("Invalid type.");
	}
	
	Shader::Shader(
		std::shared_ptr<VulkanContext> context,
		const render::ShaderType type,
		const std::vector<uint8_t>& data,
		std::vector<render::ShaderBinding> bindings
	) :
		m_context(std::move(context)),
		m_module(m_context->createShaderModule(data)),
		m_bindings(std::move(bindings)),
		m_stage(toVulkan(type))
	{
		std::vector<vk::DescriptorSetLayoutBinding> layoutBindings;
		layoutBindings.reserve(m_bindings.size());
		for (const auto& binding : m_bindings)
			layoutBindings.emplace_back(layoutBindings.size(), vk::DescriptorType::eUniformBuffer, 1, m_stage);
		m_layoutDesc = m_context->createDescriptorSetLayout(layoutBindings);
	}
}
