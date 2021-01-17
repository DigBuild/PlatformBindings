#pragma once
#include "context.h"
#include "../render_context.h"

namespace digbuild::platform::desktop::vulkan
{
	class RenderContext final : public desktop::RenderContext
	{
	public:
		explicit RenderContext(
			const RenderSurface& surface,
			std::shared_ptr<VulkanContext>&& context,
			vk::UniqueSurfaceKHR&& vkSurface
		);
		~RenderContext() override = default;
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
		
		vk::UniqueSwapchainKHR m_swapChain;

		friend class RenderManager;
	};
}
