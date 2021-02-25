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

	vk::DescriptorType toVulkan(const render::ShaderBindingType type)
	{
		switch (type)
		{
		case render::ShaderBindingType::UNIFORM:
			return vk::DescriptorType::eUniformBufferDynamic;
		case render::ShaderBindingType::SAMPLER:
			return vk::DescriptorType::eCombinedImageSampler;
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
		m_layoutDesc.reserve(m_bindings.size());
		m_layoutDesc2.reserve(m_bindings.size());
		for (const auto& binding : m_bindings)
		{
			auto layout = m_context->createDescriptorSetLayout({
				static_cast<uint32_t>(m_layoutDesc.size()),
				toVulkan(binding.type),
				1,
				m_stage
			});
			m_layoutDesc2.push_back(*layout);
			m_layoutDesc.push_back(std::move(layout));
		}
	}
}
