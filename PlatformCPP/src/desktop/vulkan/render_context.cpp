#include "render_context.h"

#include "../render_surface.h"

namespace digbuild::platform::desktop::vulkan
{
	const vk::Fence NULL_FENCE = static_cast<vk::Fence>(nullptr);
	
	RenderContext::RenderContext(
		const RenderSurface& surface, 
		std::shared_ptr<VulkanContext>&& context,
		vk::UniqueSurfaceKHR&& vkSurface,
		const render::RenderSurfaceUpdateFunction& update
	) :
		m_surface(surface),
		m_context(std::move(context)),
		m_vkSurface(std::move(vkSurface)),
		m_update(update)
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
		
		const auto stages = swapChainDesc.getOptimalImageCount();
		const auto surfaceFormat = swapChainDesc.getOptimalFormat();
		const auto surfaceExtent = swapChainDesc.getOptimalExtent(m_surface.getWidth(), m_surface.getHeight());
		
		m_swapChain = m_context->createSwapChain(
			*m_vkSurface,
			stages,
			surfaceFormat,
			swapChainDesc.getOptimalPresentMode(),
			surfaceExtent,
			swapChainDesc.getTransform(),
			*m_swapChain
		);

		m_imageViews = m_context->createSwapChainViews(*m_swapChain, surfaceFormat.format);

		m_renderPass = m_context->createSimpleRenderPass({
			{ surfaceFormat.format, vk::ImageLayout::ePresentSrcKHR }
		});
		m_framebuffer = m_context->createStagedFramebuffer(*m_renderPass, surfaceExtent, m_imageViews);

		m_commandBuffer = m_context->createCommandBuffer(stages, vk::CommandBufferLevel::ePrimary);

		m_maxFramesInFlight = stages - 1;
		m_imageAvailableSemaphore = m_context->createSemaphore(m_maxFramesInFlight);
		m_renderFinishedSemaphore = m_context->createSemaphore(m_maxFramesInFlight);
		m_inFlightFence = m_context->createFence(m_maxFramesInFlight, true);
		m_inFlightImages = std::vector<vk::Fence>(stages, NULL_FENCE);
		// m_currentFrame = 0;

		for (int i = 0u; i < stages; ++i)
		{
			const auto& cmd = m_commandBuffer[i];
			cmd.begin(vk::CommandBufferBeginInfo{});
			
			const std::vector<vk::ClearValue> clearValues = {
				vk::ClearColorValue{ std::array<float, 4>{ 0.0f, 0.0f, 0.0f, 1.0f } },
			};
			const vk::RenderPassBeginInfo renderPassBeginInfo{
				*m_renderPass,
				m_framebuffer[i],
				{ { 0, 0 }, surfaceExtent },
				static_cast<uint32_t>(clearValues.size()), clearValues.data()
			};
			cmd.beginRenderPass(&renderPassBeginInfo, vk::SubpassContents::eSecondaryCommandBuffers);
			cmd.endRenderPass();

			cmd.end();
		}
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
		const auto imageIndex = acquireResult.value;

		// Call user-provided update function
		m_update(m_surface, *this);

		if (m_inFlightImages[imageIndex] != NULL_FENCE)
			m_context->wait(m_inFlightImages[imageIndex]);
		const auto inFlight = m_inFlightImages[imageIndex] = m_inFlightFence[m_currentFrame];
		m_context->reset(inFlight);

		m_context->submit(
			m_commandBuffer[imageIndex],
			m_imageAvailableSemaphore[m_currentFrame],
			m_renderFinishedSemaphore[m_currentFrame],
			inFlight
		);
		const auto presentResult = m_context->present(
			m_renderFinishedSemaphore[m_currentFrame],
			*m_swapChain,
			imageIndex
		);

		// TODO: Check present result and potentially recreate all live resources

		m_currentFrame = (m_currentFrame + 1) % m_maxFramesInFlight;
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
