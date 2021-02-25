#pragma once
#include <queue>

#include "vk_command_buffer.h"
#include "vk_context.h"
#include "vk_framebuffer.h"
#include "vk_framebuffer_format.h"
#include "vk_texture_binding.h"
#include "vk_uniform_buffer.h"
#include "vk_vertex_buffer.h"
#include "../dt_render_context.h"
#include "../../render/render_surface.h"

namespace digbuild::platform::desktop::vulkan
{
	class RenderQueue final
	{
	public:
		void clear();
		void enqueue(std::shared_ptr<render::IRenderTarget> target, std::shared_ptr<CommandBuffer> commandBuffer);

		void write(vk::CommandBuffer& cmd);

	private:
		std::vector<std::tuple<std::shared_ptr<render::IRenderTarget>, std::shared_ptr<CommandBuffer>>> m_queue;
	};
	
	class RenderContext final : public desktop::RenderContext
	{
	public:
		explicit RenderContext(
			const RenderSurface& surface,
			std::shared_ptr<VulkanContext>&& context,
			vk::UniqueSurfaceKHR&& vkSurface,
			render::RenderSurfaceUpdateFunction update
		);
		~RenderContext() override;
		RenderContext(const RenderContext& other) = delete;
		RenderContext(RenderContext&& other) noexcept = delete;
		RenderContext& operator=(const RenderContext& other) = delete;
		RenderContext& operator=(RenderContext&& other) noexcept = delete;

		void createSwapchain();
		
		void update() override;

		[[nodiscard]] std::shared_ptr<render::FramebufferFormat> getSurfaceFormat() override
		{
			return m_surfaceFormat;
		}
		
		[[nodiscard]] std::shared_ptr<render::FramebufferFormat> createFramebufferFormat(
			const std::vector<render::FramebufferAttachmentDescriptor>& attachments,
			const std::vector<render::FramebufferRenderStageDescriptor>& renderStages
		) override;
		
		[[nodiscard]] std::shared_ptr<render::Framebuffer> createFramebuffer(
			const std::shared_ptr<render::FramebufferFormat>& format,
			uint32_t width,
			uint32_t height
		) override;

		[[nodiscard]] std::shared_ptr<render::Shader> createShader(
			render::ShaderType type,
			const std::vector<uint8_t>& data,
			const std::vector<render::ShaderBinding>& bindings
		) override;
		
		[[nodiscard]] std::shared_ptr<render::RenderPipeline> createPipeline(
			const std::shared_ptr<render::FramebufferFormat>& format,
			uint32_t stage,
			const std::vector<std::shared_ptr<render::Shader>>& shaders,
			const render::VertexFormatDescriptor& vertexFormat,
			const render::VertexFormatDescriptor& instanceFormat,
			render::RenderState state,
			const std::vector<render::BlendOptions>& blendOptions
		) override;

		[[nodiscard]] std::shared_ptr<render::UniformBuffer> createUniformBuffer(
			const std::shared_ptr<render::Shader>& shader,
			uint32_t binding,
			const std::vector<uint8_t>& initialData
		) override;

		[[nodiscard]] std::shared_ptr<render::VertexBuffer> createVertexBuffer(
			const std::vector<uint8_t>& initialData,
			uint32_t vertexSize,
			bool writable
		) override;

		[[nodiscard]] std::shared_ptr<render::TextureBinding> createTextureBinding(
			const std::shared_ptr<render::Shader>& shader,
			uint32_t binding,
			const std::shared_ptr<render::TextureSampler>& sampler,
			const std::shared_ptr<render::Texture>& texture
		) override;
		
		[[nodiscard]] std::shared_ptr<render::TextureSampler> createTextureSampler(
			render::TextureFiltering minFiltering,
			render::TextureFiltering magFiltering,
			render::TextureWrapping wrapping,
			render::TextureBorderColor borderColor,
			bool enableAnisotropy,
			uint32_t anisotropyLevel
		) override;
		
		[[nodiscard]] std::shared_ptr<render::CommandBuffer> createCommandBuffer(
		) override;
		
		void enqueue(
			const std::shared_ptr<render::IRenderTarget>& renderTarget,
			const std::shared_ptr<render::CommandBuffer>& commandBuffer
		) override;

		[[nodiscard]] render::Framebuffer& getFramebuffer() override
		{
			return *m_framebuffer;
		}

	private:
		void addTicking(std::weak_ptr<DynamicVertexBuffer> resource);
		void addTicking(std::weak_ptr<UniformBuffer> resource);
		void addTicking(std::weak_ptr<TextureBinding> resource);
		void addTicking(std::weak_ptr<CommandBuffer> resource);
		void visitTicking();
		
		const RenderSurface& m_surface;
		const std::shared_ptr<VulkanContext> m_context;
		const vk::UniqueSurfaceKHR m_vkSurface;
		const render::RenderSurfaceUpdateFunction m_update;
		
		vk::UniqueSwapchainKHR m_swapChain;
		uint32_t m_swapChainStages;
		std::shared_ptr<FramebufferFormat> m_surfaceFormat;
		std::shared_ptr<Framebuffer> m_framebuffer;
		util::StagingResource<vk::CommandBuffer> m_commandBuffer;
		std::vector<RenderQueue> m_renderQueues;

		uint32_t m_maxFramesInFlight;
		util::StagingResource<vk::Semaphore> m_imageAvailableSemaphore;
		util::StagingResource<vk::Semaphore> m_renderFinishedSemaphore;
		util::StagingResource<vk::Fence> m_inFlightFence;
		std::vector<vk::Fence> m_inFlightImages;
		uint32_t m_currentFrame = 0;
		uint32_t m_imageIndex = 0;

		std::vector<std::weak_ptr<DynamicVertexBuffer>> m_tickingVertexBuffers;
		std::queue<uint32_t> m_availableTickingVertexBufferSlots;
		std::vector<std::weak_ptr<UniformBuffer>> m_tickingUniformBuffers;
		std::queue<uint32_t> m_availableTickingUniformBufferSlots;
		std::vector<std::weak_ptr<TextureBinding>> m_tickingTextureBindings;
		std::queue<uint32_t> m_availableTickingTextureBindingSlots;
		std::vector<std::weak_ptr<CommandBuffer>> m_tickingCommandBuffers;
		std::queue<uint32_t> m_availableTickingCommandBufferSlots;

		friend class RenderManager;
	};
}
