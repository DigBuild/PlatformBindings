#include "render_context.h"

namespace digbuild::platform::desktop::vulkan
{
	RenderContext::RenderContext(
		const RenderSurface& surface, 
		std::shared_ptr<VulkanContext>&& context
	) :
		m_context(std::move(context))
	{
	}

	void RenderContext::update()
	{
	}

	std::shared_ptr<render::FramebufferFormat> RenderContext::createFramebufferFormat(
		const std::vector<render::FramebufferColorAttachmentDescriptor>& colorAttachments,
		const std::vector<render::FramebufferDepthStencilAttachmentDescriptor>& depthStencilAttachments,
		const std::vector<render::FramebufferRenderStageDescriptor>& renderStages
	)
	{
		return nullptr;
	}

	std::shared_ptr<render::Framebuffer> RenderContext::createFramebuffer(
		const std::shared_ptr<render::FramebufferFormat>& format,
		uint32_t width,
		uint32_t height
	)
	{
		return nullptr;
	}

	std::shared_ptr<render::Shader> RenderContext::createShader(
		render::ShaderType type,
		uint32_t uniformSize
	)
	{
		return nullptr;
	}

	std::shared_ptr<render::RenderPipeline> RenderContext::createPipeline(
		const std::shared_ptr<render::FramebufferFormat>& format,
		uint32_t stage,
		const std::shared_ptr<render::Shader>& vertexShader,
		const std::shared_ptr<render::Shader>& fragmentShader,
		const std::vector<render::VertexFormatDescriptor>& vertexFormat,
		render::RenderPipelineStateDescriptor state
	)
	{
		return nullptr;
	}

	std::shared_ptr<render::VertexBuffer> RenderContext::createVertexBuffer(
		const std::vector<char>& initialData,
		bool isMutable
	)
	{
		return nullptr;
	}

	std::shared_ptr<render::DrawCommand> RenderContext::createDrawCommand()
	{
		return nullptr;
	}

	void RenderContext::enqueue(
		const std::shared_ptr<render::DrawCommand>& command
	)
	{
	}
}
