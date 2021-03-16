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
			const std::vector<render::BlendOptions>& blendOptions
		);

		[[nodiscard]] vk::Pipeline& get()
		{
			return *m_pipeline;
		}

		[[nodiscard]] vk::PipelineLayout& getLayout()
		{
			return *m_layout;
		}

		[[nodiscard]] uint32_t getLayoutOffset(const std::shared_ptr<Shader>& shader) const
		{
			return m_shaderLayoutOffsets.at(shader.get());
		}
	
	private:
		std::shared_ptr<VulkanContext> m_context;
		std::shared_ptr<FramebufferFormat> m_format;
		std::vector<std::shared_ptr<Shader>> m_shaders;
		std::unordered_map<Shader*, uint32_t> m_shaderLayoutOffsets;
		std::vector<vk::DescriptorSetLayout> m_descriptorSetLayouts;
		vk::UniquePipelineLayout m_layout;
		vk::UniquePipeline m_pipeline;
	};
}
