#include "vk_command_buffer.h"

#include "vk_framebuffer_format.h"
#include "vk_render_pipeline.h"
#include "vk_texture.h"
#include "vk_texture_sampler.h"
#include "vk_uniform_buffer.h"
#include "vk_vertex_buffer.h"

namespace digbuild::platform::desktop::vulkan
{
	void CBCmdBegin::record(
		vk::CommandBuffer& cmd,
		std::vector<std::shared_ptr<render::Resource>>& resources,
		const uint32_t stage
	)
	{
		vk::CommandBufferInheritanceInfo inheritanceInfo{ m_format->getPass() };
		cmd.begin({ vk::CommandBufferUsageFlagBits::eRenderPassContinue | vk::CommandBufferUsageFlagBits::eSimultaneousUse, &inheritanceInfo });
	}

	void CBCmdEnd::record(
		vk::CommandBuffer& cmd,
		std::vector<std::shared_ptr<render::Resource>>& resources,
		const uint32_t stage
	)
	{
		cmd.end();
	}

	void CBCmdSetViewportScissor::record(
		vk::CommandBuffer& cmd,
		std::vector<std::shared_ptr<render::Resource>>& resources,
		const uint32_t stage
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
		std::vector<std::shared_ptr<render::Resource>>& resources,
		const uint32_t stage
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
		std::vector<std::shared_ptr<render::Resource>>& resources,
		const uint32_t stage
	)
	{
		cmd.setScissor(0, vk::Rect2D{
			{ static_cast<int32_t>(m_extents.x), static_cast<int32_t>(m_extents.y) },
			{ m_extents.width, m_extents.height }
		});
	}

	void CBCmdBindUniform::record(
		vk::CommandBuffer& cmd,
		std::vector<std::shared_ptr<render::Resource>>& resources,
		const uint32_t stage
	)
	{
		const auto pipeline = std::static_pointer_cast<RenderPipeline>(m_pipeline);
		const auto ub = std::static_pointer_cast<UniformBuffer>(m_uniformBuffer);
		const auto shader = std::static_pointer_cast<Shader>(m_shader);

		const vk::DescriptorBufferInfo bufferInfo {
			ub->getBuffer(),
			0,
			pipeline->getUniformSize(shader, m_binding)
		};
		vk::WriteDescriptorSet write{
			pipeline->getDescriptorSet(shader, m_binding, stage),
			0, 0,
			1,
			vk::DescriptorType::eUniformBufferDynamic,
			nullptr, &bufferInfo, nullptr
		};
		cmd.pushDescriptorSetKHR(
			vk::PipelineBindPoint::eGraphics,
			pipeline->getLayout(),
			pipeline->getActualUniform(shader, m_binding),
			1, &write
		);

		resources.push_back(pipeline);
		resources.push_back(ub);
	}

	void CBCmdBindTexture::record(
		vk::CommandBuffer& cmd,
		std::vector<std::shared_ptr<render::Resource>>& resources,
		const uint32_t stage
	)
	{
		const auto pipeline = std::static_pointer_cast<RenderPipeline>(m_pipeline);
		const auto sampler = std::static_pointer_cast<TextureSampler>(m_sampler);
		const auto texture = std::static_pointer_cast<Texture>(m_texture);
		const auto shader = std::static_pointer_cast<Shader>(m_shader);

		const vk::DescriptorImageInfo imageInfo {
			sampler->get(),
			texture->get(),
			vk::ImageLayout::eShaderReadOnlyOptimal
		};
		vk::WriteDescriptorSet write{
			pipeline->getDescriptorSet(shader, m_binding, stage),
			0, 0,
			1,
			vk::DescriptorType::eCombinedImageSampler,
			&imageInfo, nullptr, nullptr
		};
		cmd.pushDescriptorSetKHR(
			vk::PipelineBindPoint::eGraphics,
			pipeline->getLayout(),
			pipeline->getActualUniform(shader, m_binding),
			1, &write
		);

		resources.push_back(pipeline);
		resources.push_back(sampler);
		resources.push_back(texture);
	}

	void CBCmdUseUniform::record(
		vk::CommandBuffer& cmd,
		std::vector<std::shared_ptr<render::Resource>>& resources,
		const uint32_t stage
	)
	{
		const auto pipeline = std::static_pointer_cast<RenderPipeline>(m_pipeline);
		const auto shader = std::static_pointer_cast<Shader>(m_shader);
		
		cmd.bindDescriptorSets(
			vk::PipelineBindPoint::eGraphics,
			pipeline->getLayout(),
			pipeline->getActualUniform(shader, m_binding), 1,
			&pipeline->getDescriptorSet(shader, m_binding, stage),
			1, &m_index
		);
	}

	void CBCmdDraw::record(
		vk::CommandBuffer& cmd,
		std::vector<std::shared_ptr<render::Resource>>& resources,
		const uint32_t stage
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
			cbCmd->record(cmd, resources, m_readIndex);

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
		std::shared_ptr<render::Shader> shader,
		uint32_t binding
	)
	{
		m_commandQueue.push_back(std::make_unique<CBCmdBindUniform>(pipeline, uniformBuffer, shader, binding));
	}
	
	void CommandBuffer::bindTexture(
		std::shared_ptr<render::RenderPipeline> pipeline,
		std::shared_ptr<render::TextureSampler> sampler,
		std::shared_ptr<render::Texture> texture,
		std::shared_ptr<render::Shader> shader,
		uint32_t binding
	)
	{
		m_commandQueue.push_back(std::make_unique<CBCmdBindTexture>(pipeline, sampler, texture, shader, binding));
	}

	void CommandBuffer::useUniform(
		const std::shared_ptr<render::RenderPipeline>& pipeline,
		std::shared_ptr<render::Shader> shader,
		const uint32_t binding,
		const uint32_t index
	)
	{
		m_commandQueue.push_back(std::make_unique<CBCmdUseUniform>(pipeline, shader, binding, index));
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
