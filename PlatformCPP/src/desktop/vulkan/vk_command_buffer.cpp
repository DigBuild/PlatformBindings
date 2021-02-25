#include "vk_command_buffer.h"

#include "vk_framebuffer_format.h"
#include "vk_render_pipeline.h"
#include "vk_texture_binding.h"
#include "vk_uniform_buffer.h"
#include "vk_vertex_buffer.h"

namespace digbuild::platform::desktop::vulkan
{
	void CBCmdBegin::record(
		vk::CommandBuffer& cmd,
		std::vector<std::shared_ptr<render::Resource>>& resources
	)
	{
		vk::CommandBufferInheritanceInfo inheritanceInfo{ m_format->getPass() };
		cmd.begin({ vk::CommandBufferUsageFlagBits::eRenderPassContinue | vk::CommandBufferUsageFlagBits::eSimultaneousUse, &inheritanceInfo });
	}

	void CBCmdEnd::record(
		vk::CommandBuffer& cmd,
		std::vector<std::shared_ptr<render::Resource>>& resources
	)
	{
		cmd.end();
	}

	void CBCmdSetViewportScissor::record(
		vk::CommandBuffer& cmd,
		std::vector<std::shared_ptr<render::Resource>>& resources
	)
	{
		auto& fb = m_renderTarget->getFramebuffer();
		
		cmd.setViewport(0, vk::Viewport{
			0, 0,
			static_cast<float>(fb.getWidth()), static_cast<float>(fb.getHeight()),
			0.0f, 1.0f
		});
		cmd.setScissor(0, vk::Rect2D{
			{ 0, 0 },
			{ fb.getWidth(), fb.getHeight() }
		});
	}

	void CBCmdSetViewport::record(
		vk::CommandBuffer& cmd,
		std::vector<std::shared_ptr<render::Resource>>& resources
	)
	{
		cmd.setViewport(0, vk::Viewport{
			static_cast<float>(m_extents.x), static_cast<float>(m_extents.y),
			static_cast<float>(m_extents.width), static_cast<float>(m_extents.height),
			0.0f, 1.0f
		});
	}

	void CBCmdSetScissor::record(
		vk::CommandBuffer& cmd,
		std::vector<std::shared_ptr<render::Resource>>& resources
	)
	{
		cmd.setScissor(0, vk::Rect2D{
			{ static_cast<int32_t>(m_extents.x), static_cast<int32_t>(m_extents.y) },
			{ m_extents.width, m_extents.height }
		});
	}

	void CBCmdBindUniform::record(
		vk::CommandBuffer& cmd,
		std::vector<std::shared_ptr<render::Resource>>& resources
	)
	{
		auto pipeline = std::static_pointer_cast<RenderPipeline>(m_pipeline);
		auto ub = std::static_pointer_cast<UniformBuffer>(m_uniformBuffer);
		
		cmd.bindDescriptorSets(
			vk::PipelineBindPoint::eGraphics,
			pipeline->getLayout(),
			pipeline->getLayoutOffset(ub->getShader()) + ub->getBinding(),
			{ ub->get() },
			{ m_binding * ub->getShader()->getBindings()[ub->getBinding()].size }
		);

		resources.push_back(pipeline);
		resources.push_back(ub);
	}

	void CBCmdBindTexture::record(
		vk::CommandBuffer& cmd,
		std::vector<std::shared_ptr<render::Resource>>& resources
	)
	{
		auto pipeline = std::static_pointer_cast<RenderPipeline>(m_pipeline);
		auto tb = std::static_pointer_cast<TextureBinding>(m_binding);

		cmd.bindDescriptorSets(
			vk::PipelineBindPoint::eGraphics,
			pipeline->getLayout(),
			pipeline->getLayoutOffset(tb->getShader()) + tb->getBinding(),
			{ tb->get() },
			{}
		);

		resources.push_back(pipeline);
		resources.push_back(tb);
	}

	void CBCmdDraw::record(
		vk::CommandBuffer& cmd,
		std::vector<std::shared_ptr<render::Resource>>& resources
	)
	{
		auto p = std::static_pointer_cast<RenderPipeline>(m_pipeline);
		cmd.bindPipeline(vk::PipelineBindPoint::eGraphics, p->get());

		auto vb = std::static_pointer_cast<VertexBuffer>(m_vertexBuffer);
		if (m_instanceBuffer)
		{
			auto ib = std::static_pointer_cast<VertexBuffer>(m_instanceBuffer);
			cmd.bindVertexBuffers(
				0,
				{ vb->get(), ib->get() },
				{ 0, 0 }
			);
			cmd.draw(vb->size(), ib->size(), 0, 0);
		}
		else
		{
			cmd.bindVertexBuffers(
				0,
				{ vb->get() },
				{ 0 }
			);
			cmd.draw(vb->size(), 1, 0, 0);
		}
		
		resources.push_back(m_pipeline);
		resources.push_back(m_vertexBuffer);
		if (m_instanceBuffer)
			resources.push_back(m_instanceBuffer);
	}

	CommandBuffer::CommandBuffer(
		std::shared_ptr<VulkanContext> context, 
		const uint32_t stages
	) :
		m_context(std::move(context))
	{
		m_commandBuffers = m_context->createCommandBuffers(stages, vk::CommandBufferLevel::eSecondary);
		m_resources.reserve(stages);
		for (auto i = 0u; i < stages; ++i)
			m_resources.emplace_back();
	}

	void CommandBuffer::tick()
	{
		const auto writeIndex = (m_readIndex + 1) % static_cast<uint32_t>(m_commandBuffers.size());

		if (m_leftoverWrites == 0)
		{
			m_readIndex = writeIndex;
			return;
		}

		auto& cmd = *m_commandBuffers[writeIndex];
		auto& resources = m_resources[writeIndex];
		resources.clear();

		for (auto& cbCmd : m_commandQueue)
			cbCmd->record(cmd, resources);

		m_leftoverWrites--;
		m_readIndex = writeIndex;
	}

	void CommandBuffer::reserve(const uint32_t stages)
	{
		const auto missing = stages - m_commandBuffers.size();
		if (missing <= 0) return;

		auto missingBuffers = m_context->createCommandBuffers(static_cast<uint32_t>(missing), vk::CommandBufferLevel::eSecondary);
		m_commandBuffers.insert(
			m_commandBuffers.begin(),
			std::make_move_iterator(missingBuffers.begin()),
			std::make_move_iterator(missingBuffers.end())
		);
	}

	void CommandBuffer::beginRecording(const std::shared_ptr<render::FramebufferFormat>& format)
	{
		m_commandQueue.clear();
		m_commandQueue.push_back(std::make_unique<CBCmdBegin>(std::static_pointer_cast<FramebufferFormat>(format)));
	}

	void CommandBuffer::setViewportAndScissor(std::shared_ptr<render::IRenderTarget> renderTarget)
	{
		m_commandQueue.push_back(std::make_unique<CBCmdSetViewportScissor>(renderTarget));
	}

	void CommandBuffer::setViewport(const platform::util::Extents2D extents)
	{
		m_commandQueue.push_back(std::make_unique<CBCmdSetViewport>(extents));
	}

	void CommandBuffer::setScissor(const platform::util::Extents2D extents)
	{
		m_commandQueue.push_back(std::make_unique<CBCmdSetScissor>(extents));
	}

	void CommandBuffer::bindUniform(
		std::shared_ptr<render::RenderPipeline> pipeline,
		std::shared_ptr<render::UniformBuffer> uniformBuffer,
		uint32_t binding
	)
	{
		m_commandQueue.push_back(std::make_unique<CBCmdBindUniform>(pipeline, uniformBuffer, binding));
	}

	void CommandBuffer::bindTexture(
		std::shared_ptr<render::RenderPipeline> pipeline,
		std::shared_ptr<render::TextureBinding> binding
	)
	{
		m_commandQueue.push_back(std::make_unique<CBCmdBindTexture>(pipeline, binding));
	}

	void CommandBuffer::draw(
		const std::shared_ptr<render::RenderPipeline> pipeline,
		const std::shared_ptr<render::VertexBuffer> vertexBuffer,
		const std::shared_ptr<render::VertexBuffer> instanceBuffer
	)
	{
		m_commandQueue.push_back(std::make_unique<CBCmdDraw>(pipeline, vertexBuffer, instanceBuffer));
	}

	void CommandBuffer::finishRecording()
	{
		m_commandQueue.push_back(std::make_unique<CBCmdEnd>());
		
		m_leftoverWrites = static_cast<uint32_t>(m_commandBuffers.size());
	}

	vk::CommandBuffer& CommandBuffer::get()
	{
		return *m_commandBuffers[m_readIndex];
	}
}
