#pragma once
#include "context.h"
#include "../render_context.h"
#include "../../render/render_surface.h"

namespace digbuild::platform::desktop::vulkan
{
	class RenderContext final : public desktop::RenderContext
	{
	public:
		explicit RenderContext(
			const RenderSurface& surface,
			std::shared_ptr<VulkanContext>&& context,
			vk::UniqueSurfaceKHR&& vkSurface,
			const render::RenderSurfaceUpdateFunction& update
		);
		~RenderContext() override;
		RenderContext(const RenderContext& other) = delete;
		RenderContext(RenderContext&& other) noexcept = delete;
		RenderContext& operator=(const RenderContext& other) = delete;
		RenderContext& operator=(RenderContext&& other) noexcept = delete;

		void createSwapchain();
		
		void update() override;
		
		[[nodiscard]] std::shared_ptr<render::FramebufferFormat> createFramebufferFormat(
			const std::vector<render::FramebufferColorAttachmentDescriptor>& colorAttachments,
			const std::vector<render::FramebufferDepthStencilAttachmentDescriptor>& depthStencilAttachments,
			const std::vector<render::FramebufferRenderStageDescriptor>& renderStages
		) override;
		
		[[nodiscard]] std::shared_ptr<render::Framebuffer> createFramebuffer(
			const std::shared_ptr<render::FramebufferFormat>& format,
			uint32_t width,
			uint32_t height
		) override;
		
		[[nodiscard]] std::shared_ptr<render::Shader> createShader(
			render::ShaderType type,
			uint32_t uniformSize
		) override;
		
		[[nodiscard]] std::shared_ptr<render::RenderPipeline> createPipeline(
			const std::shared_ptr<render::FramebufferFormat>& format,
			uint32_t stage,
			const std::shared_ptr<render::Shader>& vertexShader,
			const std::shared_ptr<render::Shader>& fragmentShader,
			const std::vector<render::VertexFormatDescriptor>& vertexFormat,
			render::RenderPipelineStateDescriptor state
		) override;
		
		[[nodiscard]] std::shared_ptr<render::VertexBuffer> createVertexBuffer(
			const std::vector<char>& initialData,
			bool isMutable
		) override;
		
		[[nodiscard]] std::shared_ptr<render::DrawCommand> createDrawCommand(
		) override;
		
		void enqueue(
			const std::shared_ptr<render::DrawCommand>& command
		) override;

	private:
		const RenderSurface& m_surface;
		const std::shared_ptr<VulkanContext> m_context;
		const vk::UniqueSurfaceKHR m_vkSurface;
		const render::RenderSurfaceUpdateFunction m_update;

		utils::StagingResource<vk::ImageView> m_imageViews;
		vk::UniqueSwapchainKHR m_swapChain;
		vk::UniqueRenderPass m_renderPass;
		utils::StagingResource<vk::Framebuffer> m_framebuffer;
		utils::StagingResource<vk::CommandBuffer> m_commandBuffer;

		uint32_t m_maxFramesInFlight;
		utils::StagingResource<vk::Semaphore> m_imageAvailableSemaphore;
		utils::StagingResource<vk::Semaphore> m_renderFinishedSemaphore;
		utils::StagingResource<vk::Fence> m_inFlightFence;
		std::vector<vk::Fence> m_inFlightImages;
		uint32_t m_currentFrame = 0;

		friend class RenderManager;
	};
}
