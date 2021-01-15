#pragma once
#include <memory>
#include <vector>

#include "draw_command.h"
#include "framebuffer.h"
#include "framebuffer_format.h"
#include "render_pipeline.h"
#include "shader.h"
#include "texture.h"
#include "vertex_buffer.h"

namespace digbuild::platform::render
{
	struct FramebufferColorAttachmentDescriptor
	{
		const TextureFormat format;
	};
	struct FramebufferDepthStencilAttachmentDescriptor
	{
		const TextureFormat format;
	};
	struct FramebufferRenderStageDescriptor
	{
		const std::vector<uint32_t> attachments;
		const std::vector<uint32_t> dependencies;
	};
	
	enum class VertexFormatElementType : uint8_t
	{
		BYTE, UBYTE,
		SHORT, USHORT,
		INT, UINT,
		LONG, ULONG,
		FLOAT, DOUBLE,
		FLOAT2, FLOAT3, FLOAT4
	};
	enum class VertexFormatDescriptorRate : uint8_t
	{
		VERTEX, INSTANCE
	};
	struct VertexFormatElement
	{
		const uint32_t offset;
		const VertexFormatElementType type;
	};
	struct VertexFormatDescriptor
	{
		const std::vector<VertexFormatElement> elements;
		const uint32_t size;
		const VertexFormatDescriptorRate rate;
	};
	struct RenderPipelineStateDescriptor
	{
		
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
		
		[[nodiscard]] virtual std::shared_ptr<RenderPipeline> createPipeline(
			const std::shared_ptr<FramebufferFormat>& format,
			uint32_t stage,
			const std::shared_ptr<Shader>& vertexShader,
			const std::shared_ptr<Shader>& fragmentShader,
			const std::vector<VertexFormatDescriptor>& vertexFormat,
			RenderPipelineStateDescriptor state
		) = 0;

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
