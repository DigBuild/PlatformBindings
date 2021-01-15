#pragma once
#include <memory>
#include <vector>

#include "draw_command.h"
#include "framebuffer.h"
#include "shader.h"
#include "texture.h"
#include "vertex_buffer.h"

namespace digbuild::platform
{
	struct FramebufferColorAttachmentDescriptor
	{
		TextureFormat format;
	};
	struct FramebufferDepthStencilAttachmentDescriptor
	{
		TextureFormat format;
	};
	struct FramebufferRenderStageDescriptor
	{
		std::vector<uint32_t> attachments;
		std::vector<uint32_t> dependencies;
	};
	
	class RenderContext
	{
	public:
		RenderContext() = default;
		virtual ~RenderContext() = default;
		RenderContext(const RenderContext& other) = delete;
		RenderContext(RenderContext&& other) noexcept = delete;
		RenderContext& operator=(const RenderContext& other) = delete;
		RenderContext& operator=(RenderContext&& other) noexcept = delete;
		
		[[nodiscard]] virtual std::shared_ptr<FramebufferFormat> createFramebufferFormat(
			const std::vector<FramebufferColorAttachmentDescriptor>& colorAttachments,
			const std::vector<FramebufferDepthStencilAttachmentDescriptor>& depthStencilAttachments,
			const std::vector<FramebufferRenderStageDescriptor>& renderStages
		) = 0;

		[[nodiscard]] virtual std::shared_ptr<Framebuffer> createFramebuffer(
			const std::shared_ptr<FramebufferFormat>& format,
			uint32_t width, uint32_t height
		) = 0;

		[[nodiscard]] virtual std::shared_ptr<Shader> createShader(
			ShaderType type,
			uint32_t uniformSize
		) = 0;

		// TODO: Render pipeline

		[[nodiscard]] virtual std::shared_ptr<VertexBuffer> createVertexBuffer(
			const std::vector<char>& initialData,
			bool isMutable
		) = 0;

		[[nodiscard]] virtual std::shared_ptr<DrawCommand> createDrawCommand(
		) = 0;

		virtual void enqueue(
			const std::shared_ptr<DrawCommand>& command
		) = 0;
	};
}
