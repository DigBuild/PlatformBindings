﻿#pragma once
#include <memory>

#include "framebuffer_format.h"
#include "render_pipeline.h"
#include "render_target.h"
#include "resource.h"
#include "shader.h"
#include "texture.h"
#include "texture_sampler.h"
#include "uniform_buffer.h"
#include "vertex_buffer.h"
#include "../util/vecmath.h"

namespace digbuild::platform::render
{
	class CommandBuffer : public Resource, public std::enable_shared_from_this<CommandBuffer>
	{
	public:
		CommandBuffer() = default;
		~CommandBuffer() override = default;
		CommandBuffer(const CommandBuffer& other) = delete;
		CommandBuffer(CommandBuffer&& other) noexcept = delete;
		CommandBuffer& operator=(const CommandBuffer& other) = delete;
		CommandBuffer& operator=(CommandBuffer&& other) noexcept = delete;

		virtual void beginRecording(const std::shared_ptr<FramebufferFormat>& format) = 0;
		virtual void setViewportAndScissor(std::shared_ptr<IRenderTarget> renderTarget) = 0;
		virtual void setViewport(util::Extents2D extents) = 0;
		virtual void setScissor(util::Extents2D extents) = 0;
		virtual void bindUniform(
			std::shared_ptr<RenderPipeline> pipeline,
			std::shared_ptr<UniformBuffer> uniformBuffer,
			std::shared_ptr<Shader> shader,
			uint32_t binding
		) = 0;
		virtual void bindTexture(
			std::shared_ptr<RenderPipeline> pipeline,
			std::shared_ptr<TextureSampler> sampler,
			std::shared_ptr<Texture> texture,
			std::shared_ptr<Shader> shader,
			uint32_t binding
		) = 0;
		virtual void useUniform(
			const std::shared_ptr<RenderPipeline>& pipeline,
			std::shared_ptr<Shader> shader,
			uint32_t binding,
			uint32_t index
		) = 0;
		virtual void draw(
			std::shared_ptr<RenderPipeline> pipeline,
			std::shared_ptr<VertexBuffer> vertexBuffer,
			std::shared_ptr<VertexBuffer> instanceBuffer
		) = 0;
		virtual void finishRecording() = 0;
	};
}
