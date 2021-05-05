#include "command_buffer.h"


#include "render_context.h"
#include "../util/native_handle.h"
#include "../util/utils.h"

namespace digbuild::platform::render
{
	enum class CommandBufferCmdTypeC : uint64_t
	{
		SET_VIEWPORT_SCISSOR,
		SET_VIEWPORT,
		SET_SCISSOR,
		BIND_UNIFORM,
		BIND_TEXTURE,
		USE_UNIFORM,
		DRAW
	};

	struct CommandBufferCmdSetViewportScissorC
	{
		const util::native_handle target;
	};
	struct CommandBufferCmdSetViewportC
	{
		const util::Extents2D extents;
	};
	struct CommandBufferCmdSetScissorC
	{
		const util::Extents2D extents;
	};
	struct CommandBufferCmdBindUniformC
	{
		const util::native_handle pipeline;
		const util::native_handle uniformBuffer;
		const util::native_handle shader;
		const uint32_t binding;
	};
	struct CommandBufferCmdBindTextureC
	{
		const util::native_handle pipeline;
		const util::native_handle sampler;
		const util::native_handle texture;
		const util::native_handle shader;
		const uint32_t binding;
	};
	struct CommandBufferCmdUseUniformC
	{
		const util::native_handle pipeline;
		const util::native_handle shader;
		const uint32_t binding;
		const uint32_t index;
	};
	struct CommandBufferCmdDrawC
	{
		const util::native_handle pipeline;
		const util::native_handle vertexBuffer;
		const util::native_handle instanceBuffer;
	};
	
	struct CommandBufferCmdC
	{
		const CommandBufferCmdTypeC type;
		union
		{
			const CommandBufferCmdSetViewportScissorC cmdSetViewportScissor;
			const CommandBufferCmdSetViewportC cmdSetViewport;
			const CommandBufferCmdSetScissorC cmdSetScissor;
			const CommandBufferCmdBindUniformC cmdBindUniform;
			const CommandBufferCmdBindTextureC cmdBindTexture;
			const CommandBufferCmdUseUniformC cmdUseUniform;
			const CommandBufferCmdDrawC cmdDraw;
		};
	};
}

using namespace digbuild::platform::util;
using namespace digbuild::platform::render;
extern "C" {
	DLLEXPORT void dbp_command_buffer_commit(
		const native_handle instance,
		RenderContext* context,
		const native_handle format,
		const CommandBufferCmdC* commands,
		const uint32_t commandCount
	)
	{
		auto* commandBuffer = handle_cast<CommandBuffer>(instance);
		auto fmt = handle_share<FramebufferFormat>(format);
		if (!fmt)
			fmt = context->getSurfaceFormat();

		commandBuffer->beginRecording(fmt);
		for (uint32_t i = 0; i < commandCount; ++i)
		{
			const auto& cmd = commands[i];
			switch (cmd.type)
			{
			case CommandBufferCmdTypeC::SET_VIEWPORT_SCISSOR:
				commandBuffer->setViewportAndScissor(
					handle_share<IRenderTarget>(cmd.cmdSetViewportScissor.target)
				);
				break;
			case CommandBufferCmdTypeC::SET_VIEWPORT:
				commandBuffer->setViewport(cmd.cmdSetViewport.extents);
				break;
			case CommandBufferCmdTypeC::SET_SCISSOR:
				commandBuffer->setScissor(cmd.cmdSetScissor.extents);
				break;
			case CommandBufferCmdTypeC::BIND_UNIFORM:
				commandBuffer->bindUniform(
					handle_share<RenderPipeline>(cmd.cmdBindUniform.pipeline),
					handle_share<UniformBuffer>(cmd.cmdBindUniform.uniformBuffer),
					handle_share<Shader>(cmd.cmdBindUniform.shader),
					cmd.cmdBindUniform.binding
				);
				break;
			case CommandBufferCmdTypeC::BIND_TEXTURE:
				commandBuffer->bindTexture(
					handle_share<RenderPipeline>(cmd.cmdBindTexture.pipeline),
					handle_share<TextureSampler>(cmd.cmdBindTexture.sampler),
					handle_share<Texture>(cmd.cmdBindTexture.texture),
					handle_share<Shader>(cmd.cmdBindTexture.shader),
					cmd.cmdBindTexture.binding
				);
				break;
			case CommandBufferCmdTypeC::USE_UNIFORM:
				commandBuffer->useUniform(
					handle_share<RenderPipeline>(cmd.cmdUseUniform.pipeline),
					handle_share<Shader>(cmd.cmdUseUniform.shader),
					cmd.cmdUseUniform.binding,
					cmd.cmdUseUniform.index
				);
				break;
			case CommandBufferCmdTypeC::DRAW:
				commandBuffer->draw(
					handle_share<RenderPipeline>(cmd.cmdDraw.pipeline),
					handle_share<VertexBuffer>(cmd.cmdDraw.vertexBuffer),
					handle_share<VertexBuffer>(cmd.cmdDraw.instanceBuffer)
				);
				break;
			}
		}
		commandBuffer->finishRecording();
	}
}

