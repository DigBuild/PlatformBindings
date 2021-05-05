#pragma once

#include "vk_context.h"
#include "vk_framebuffer_format.h"
#include "vk_shader.h"
#include "../../render/render_pipeline.h"

namespace digbuild::platform::desktop::vulkan
{
	class RenderPipeline final : public render::RenderPipeline
	{
	public:
		RenderPipeline(
			std::shared_ptr<VulkanContext> context,
			std::shared_ptr<FramebufferFormat> format,
			uint32_t stage,
			std::vector<std::shared_ptr<Shader>> shaders,
			const render::VertexFormatDescriptor& vertexFormat,
			const render::VertexFormatDescriptor& instanceFormat,
			render::RenderState state,
			const std::vector<render::BlendOptions>& blendOptions,
			uint32_t stages
		);

		[[nodiscard]] const vk::Pipeline& get() const
		{
			return *m_pipeline;
		}

		[[nodiscard]] const vk::PipelineLayout& getLayout() const
		{
			return *m_layout;
		}

		[[nodiscard]] uint32_t getActualUniform(const std::shared_ptr<Shader>& shader, uint32_t binding) const;

		[[nodiscard]] const vk::DescriptorSet& getDescriptorSet(const std::shared_ptr<Shader>& shader, const uint32_t binding, const uint32_t stage) const
		{
			return *m_descriptorSets[getActualUniform(shader, binding)][stage];
		}

		[[nodiscard]] uint32_t getUniformSize(const std::shared_ptr<Shader>& shader, const uint32_t binding) const
		{
			return m_bindingSizes[getActualUniform(shader, binding)];
		}

	private:
		std::shared_ptr<VulkanContext> m_context;
		std::shared_ptr<FramebufferFormat> m_format;
		std::vector<std::shared_ptr<Shader>> m_shaders;

		std::vector<uint32_t> m_shaderOffsets;
		std::vector<vk::UniqueDescriptorSetLayout> m_descriptorSetLayouts;
		std::vector<vk::UniqueDescriptorPool> m_descriptorPools;
		std::vector<std::vector<vk::UniqueDescriptorSet>> m_descriptorSets;
		vk::UniquePipelineLayout m_layout;
		std::vector<uint32_t> m_bindingSizes;
		
		vk::UniquePipeline m_pipeline;
	};
}
