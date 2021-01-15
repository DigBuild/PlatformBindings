#pragma once
#include <memory>

#include "framebuffer.h"
#include "render_pipeline.h"
#include "vertex_buffer.h"
#include "../util/vecmath.h"

namespace digbuild::platform::render
{
	class DrawCommand : public std::enable_shared_from_this<DrawCommand>
	{
	public:
		DrawCommand() = default;
		virtual ~DrawCommand() = default;
		DrawCommand(const DrawCommand& other) = delete;
		DrawCommand(DrawCommand&& other) noexcept = delete;
		DrawCommand& operator=(const DrawCommand& other) = delete;
		DrawCommand& operator=(DrawCommand&& other) noexcept = delete;

		virtual void beginRecording() = 0;
		virtual void finishRecording() = 0;

		virtual void setAndClearRenderTarget(
			const std::shared_ptr<Framebuffer>& target,
			const std::vector<util::Vector4>& clearColors
		) = 0;
		
		virtual void draw(
			const std::shared_ptr<RenderPipeline>& pipeline,
			const std::shared_ptr<VertexBuffer>& vertexData,
			const std::shared_ptr<VertexBuffer>& instanceData
		) = 0;
	};
}
