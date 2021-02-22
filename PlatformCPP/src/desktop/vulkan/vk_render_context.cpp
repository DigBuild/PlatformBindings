#include "vk_render_context.h"

#include "vk_command_buffer.h"
#include "vk_render_pipeline.h"
#include "vk_shader.h"
#include "vk_uniform_buffer.h"
#include "vk_vertex_buffer.h"
#include "../dt_render_surface.h"

namespace digbuild::platform::desktop::vulkan
{
	const vk::Fence NULL_FENCE = static_cast<vk::Fence>(nullptr);

	void RenderQueue::clear()
	{
		m_queue.clear();
	}

	void RenderQueue::enqueue(
		std::shared_ptr<render::IRenderTarget> target,
	    std::shared_ptr<CommandBuffer> commandBuffer
	)
	{
		m_queue.emplace_back(std::move(target), std::move(commandBuffer));
	}

	void RenderQueue::write(vk::CommandBuffer& cmd)
	{
		cmd.begin(vk::CommandBufferBeginInfo{});
		for (auto& [target, buffer] : m_queue)
		{
			auto& framebuffer = reinterpret_cast<Framebuffer&>(target->getFramebuffer());
			const auto& format = reinterpret_cast<const FramebufferFormat&>(framebuffer.getFormat());
			
			cmd.beginRenderPass(
				{
					format.getPass(),
					framebuffer.get(),
					{
						{0, 0},
						{framebuffer.getWidth(), framebuffer.getHeight()}
					},
					std::vector<vk::ClearValue>{
						vk::ClearColorValue { std::array<float, 4>{ 0.3f, 0.1f, 0.0f, 1.0f } }
					} // TODO: Clear values
				},
				vk::SubpassContents::eSecondaryCommandBuffers
			);
			
			cmd.executeCommands({ buffer->get() });
			
			cmd.endRenderPass();
		
			// cmd.pipelineBarrier(
			// 	vk::PipelineStageFlagBits::eBottomOfPipe,
			// 	vk::PipelineStageFlagBits::eTopOfPipe,
			// 	vk::DependencyFlagBits::eDeviceGroup,
			// 	{ }, { }, { }
			// );
		}
		cmd.end();
	}

	RenderContext::RenderContext(
		const RenderSurface& surface, 
		std::shared_ptr<VulkanContext>&& context,
		vk::UniqueSurfaceKHR&& vkSurface,
		render::RenderSurfaceUpdateFunction update
	) :
		m_surface(surface),
		m_context(std::move(context)),
		m_vkSurface(std::move(vkSurface)),
		m_update(std::move(update)),
		m_swapChainStages(0),
		m_maxFramesInFlight(0)
	{
		createSwapchain();
	}

	RenderContext::~RenderContext()
	{
		m_context->waitIdle();
	}

	void RenderContext::createSwapchain()
	{
		const auto swapChainDesc = m_context->getSwapChainDescriptor(*m_vkSurface);
		
		m_swapChainStages = swapChainDesc.getOptimalImageCount();
		const auto surfaceFormat = swapChainDesc.getOptimalFormat();
		const auto surfaceExtent = swapChainDesc.getOptimalExtent(m_surface.getWidth(), m_surface.getHeight());
		
		m_swapChain = m_context->createSwapChain(
			*m_vkSurface,
			m_swapChainStages,
			surfaceFormat,
			swapChainDesc.getOptimalPresentMode(),
			surfaceExtent,
			swapChainDesc.getTransform(),
			*m_swapChain
		);

		m_commandBuffer = m_context->createCommandBuffer(m_swapChainStages, vk::CommandBufferLevel::ePrimary);
		m_renderQueues.clear();
		m_renderQueues.reserve(m_swapChainStages);
		for (auto i = 0u; i < m_swapChainStages; ++i)
			m_renderQueues.emplace_back();

		m_maxFramesInFlight = m_swapChainStages - 1;
		m_imageAvailableSemaphore = m_context->createSemaphore(m_maxFramesInFlight);
		m_renderFinishedSemaphore = m_context->createSemaphore(m_maxFramesInFlight);
		m_inFlightFence = m_context->createFence(m_maxFramesInFlight, true);
		m_inFlightImages = std::vector(m_swapChainStages, NULL_FENCE);
		// m_currentFrame = 0;
		
		auto renderPass = m_context->createSimpleRenderPass({
			{ surfaceFormat.format, vk::ImageLayout::ePresentSrcKHR }
		});
		m_surfaceFormat = std::make_shared<FramebufferFormat>(m_context, std::move(renderPass), 1);
		
		auto imageViews = m_context->createSwapChainViews(*m_swapChain, surfaceFormat.format);
		auto framebuffers = m_context->createFramebuffers(m_surfaceFormat->getPass(), surfaceExtent, imageViews);
		m_framebuffer = std::make_shared<Framebuffer>(
			m_surfaceFormat,
			surfaceExtent.width, surfaceExtent.height,
			std::move(imageViews), std::move(framebuffers)
		);

		// for (auto i = 0u; i < m_swapChainStages; ++i)
		// {
		// 	const auto& cmd = m_commandBuffer[i];
		// 	cmd.begin(vk::CommandBufferBeginInfo{});
		// 	
		// 	const std::vector<vk::ClearValue> clearValues = {
		// 		vk::ClearColorValue{ std::array<float, 4>{ 1.0f, 0.0f, 0.0f, 1.0f } },
		// 	};
		// 	const vk::RenderPassBeginInfo renderPassBeginInfo{
		// 		m_framebuffer->getFormat().getPass(),
		// 		(*m_framebuffer)[i],
		// 		{ { 0, 0 }, surfaceExtent },
		// 		static_cast<uint32_t>(clearValues.size()), clearValues.data()
		// 	};
		// 	cmd.beginRenderPass(&renderPassBeginInfo, vk::SubpassContents::eSecondaryCommandBuffers);
		// 	cmd.endRenderPass();
		// 	
		// 	cmd.end();
		// }
	}

	void RenderContext::update()
	{
		m_context->wait(m_inFlightFence[m_currentFrame]);

		const auto acquireResult = m_context->acquireNextImage(*m_swapChain, m_imageAvailableSemaphore[m_currentFrame]);
		if (acquireResult.result == vk::Result::eErrorOutOfDateKHR)
		{
			// TODO: recreate live resources
			return;
		}
		m_imageIndex = acquireResult.value;

		auto& queue = m_renderQueues[m_imageIndex];
		queue.clear();

		// Call user-provided update function
		m_update(m_surface, *this);

		visitTicking();

		if (m_inFlightImages[m_imageIndex] != NULL_FENCE)
			m_context->wait(m_inFlightImages[m_imageIndex]);
		const auto inFlight = m_inFlightImages[m_imageIndex] = m_inFlightFence[m_currentFrame];
		m_context->reset(inFlight);

		auto& cb = m_commandBuffer[m_imageIndex];
		queue.write(cb);

		m_context->submit(
			cb,
			m_imageAvailableSemaphore[m_currentFrame],
			m_renderFinishedSemaphore[m_currentFrame],
			inFlight
		);
		const auto presentResult = m_context->present(
			m_renderFinishedSemaphore[m_currentFrame],
			*m_swapChain,
			m_imageIndex
		);

		// TODO: Check present result and potentially recreate all live resources

		m_currentFrame = (m_currentFrame + 1) % m_maxFramesInFlight;
		m_framebuffer->next();
	}

	std::shared_ptr<render::FramebufferFormat> RenderContext::createFramebufferFormat(
		const std::vector<render::FramebufferAttachmentDescriptor>& attachments,
		const std::vector<render::FramebufferRenderStageDescriptor>& renderStages
	)
	{
		return std::make_shared<FramebufferFormat>(m_context, attachments, renderStages);
	}

	std::shared_ptr<render::Framebuffer> RenderContext::createFramebuffer(
		const std::shared_ptr<render::FramebufferFormat>& format,
		const uint32_t width,
		const uint32_t height
	)
	{
		return nullptr;
	}

	std::shared_ptr<render::Shader> RenderContext::createShader(
		const render::ShaderType type,
		const std::vector<uint8_t>& data,
		const std::vector<render::ShaderBinding>& bindings
	)
	{
		return std::make_shared<Shader>(m_context, type, data, bindings);
	}

	std::shared_ptr<render::RenderPipeline> RenderContext::createPipeline(
		const std::shared_ptr<render::FramebufferFormat>& format,
		const uint32_t stage,
		const std::vector<std::shared_ptr<render::Shader>>& shaders,
		const render::VertexFormatDescriptor& vertexFormat,
		const render::VertexFormatDescriptor& instanceFormat,
		const render::RenderState state,
		const std::vector<render::BlendOptions>& blendOptions
	)
	{
		std::vector<std::shared_ptr<Shader>> vkShaders;
		vkShaders.reserve(shaders.size());
		for (const auto& shader : shaders)
			vkShaders.push_back(std::static_pointer_cast<Shader>(shader));
		
		return std::make_shared<RenderPipeline>(
			m_context,
			std::static_pointer_cast<FramebufferFormat>(format),
			stage, vkShaders,
			vertexFormat, instanceFormat,
			state, blendOptions
		);
	}

	std::shared_ptr<render::UniformBuffer> RenderContext::createUniformBuffer(
		const std::shared_ptr<render::Shader>& shader,
		uint32_t binding,
		const std::vector<uint8_t>& initialData
	)
	{
		auto ub = std::make_shared<UniformBuffer>(
			m_context,
			std::static_pointer_cast<Shader>(shader),
			binding,
			m_swapChainStages,
			initialData
		);
		addTicking(ub);
		return std::move(ub);
	}

	std::shared_ptr<render::VertexBuffer> RenderContext::createVertexBuffer(
		const std::vector<uint8_t>& initialData,
		const uint32_t vertexSize,
		const bool writable
	)
	{
		if (writable)
		{
			auto vb = std::make_shared<DynamicVertexBuffer>(
				m_context,
				initialData,
				vertexSize,
				m_swapChainStages
			);
			addTicking(vb);
			return std::move(vb);
		}

		return std::make_shared<StaticVertexBuffer>(
			m_context,
			initialData,
			vertexSize
		);
	}


	std::shared_ptr<render::CommandBuffer> RenderContext::createCommandBuffer()
	{
		auto cmd = std::make_shared<CommandBuffer>(
			m_context,
			m_swapChainStages
		);
		addTicking(cmd);
		return std::move(cmd);
	}

	void RenderContext::enqueue(
		const std::shared_ptr<render::IRenderTarget>& renderTarget,
		const std::shared_ptr<render::CommandBuffer>& commandBuffer
	)
	{
		m_renderQueues[m_imageIndex].enqueue(
			renderTarget,
			std::static_pointer_cast<CommandBuffer>(commandBuffer)
		);
	}

	void RenderContext::addTicking(std::weak_ptr<DynamicVertexBuffer> resource)
	{
		if (m_availableTickingVertexBufferSlots.empty())
			return m_tickingVertexBuffers.push_back(std::move(resource));

		const auto slot = m_availableTickingVertexBufferSlots.front();
		m_availableTickingVertexBufferSlots.pop();
		m_tickingVertexBuffers[slot] = std::move(resource);
	}

	void RenderContext::addTicking(std::weak_ptr<UniformBuffer> resource)
	{
		if (m_availableTickingUniformBufferSlots.empty())
			return m_tickingUniformBuffers.push_back(std::move(resource));

		const auto slot = m_availableTickingUniformBufferSlots.front();
		m_availableTickingUniformBufferSlots.pop();
		m_tickingUniformBuffers[slot] = std::move(resource);
	}

	void RenderContext::addTicking(std::weak_ptr<CommandBuffer> resource)
	{
		if (m_availableTickingCommandBufferSlots.empty())
			return m_tickingCommandBuffers.push_back(std::move(resource));

		const auto slot = m_availableTickingCommandBufferSlots.front();
		m_availableTickingCommandBufferSlots.pop();
		m_tickingCommandBuffers[slot] = std::move(resource);
	}

	void RenderContext::visitTicking()
	{
		uint32_t i = 0;
		for (auto& res : m_tickingVertexBuffers)
		{
			if (res.expired())
			{
				m_availableTickingVertexBufferSlots.emplace(i);
				i++;
				continue;
			}

			res.lock()->tick();
			i++;
		}

		i = 0;
		for (auto& res : m_tickingUniformBuffers)
		{
			if (res.expired())
			{
				m_availableTickingUniformBufferSlots.emplace(i);
				i++;
				continue;
			}

			res.lock()->tick();
			i++;
		}

		i = 0;
		for (auto& res : m_tickingCommandBuffers)
		{
			if (res.expired())
			{
				m_availableTickingCommandBufferSlots.emplace(i);
				i++;
				continue;
			}

			res.lock()->tick();
			i++;
		}
	}
}
