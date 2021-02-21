#pragma once
#include "vk_context.h"
#include "../../render/command_buffer.h"

namespace digbuild::platform::desktop::vulkan
{
	class CBCmd
	{
	public:
		CBCmd() = default;
		virtual ~CBCmd() = default;
		CBCmd(const CBCmd& other) = delete;
		CBCmd(CBCmd&& other) noexcept = delete;
		CBCmd& operator=(const CBCmd& other) = delete;
		CBCmd& operator=(CBCmd&& other) noexcept = delete;
		
		virtual void record(
			vk::CommandBuffer& cmd,
			std::vector<std::shared_ptr<render::Resource>>& resources
		) = 0;
	};

	class CBCmdBegin final : public CBCmd
	{
	public:
		explicit CBCmdBegin(std::shared_ptr<FramebufferFormat> format) :
			m_format(std::move(format)) { }

		void record(
			vk::CommandBuffer& cmd,
			std::vector<std::shared_ptr<render::Resource>>& resources
		) override;
	private:
		std::shared_ptr<FramebufferFormat> m_format;
	};
	class CBCmdEnd final : public CBCmd
	{
	public:
		explicit CBCmdEnd() { }

		void record(
			vk::CommandBuffer& cmd,
			std::vector<std::shared_ptr<render::Resource>>& resources
		) override;
	};
	class CBCmdSetViewportScissor final : public CBCmd
	{
	public:
		explicit CBCmdSetViewportScissor(std::shared_ptr<render::IRenderTarget> renderTarget) :
			m_renderTarget(std::move(renderTarget)) { }

		void record(
			vk::CommandBuffer& cmd,
			std::vector<std::shared_ptr<render::Resource>>& resources
		) override;
	private:
		std::shared_ptr<render::IRenderTarget> m_renderTarget;
	};
	class CBCmdSetViewport final : public CBCmd
	{
	public:
		explicit CBCmdSetViewport(const platform::util::Extents2D extents) :
			m_extents(extents) { }

		void record(
			vk::CommandBuffer& cmd,
			std::vector<std::shared_ptr<render::Resource>>& resources
		) override;
	private:
		platform::util::Extents2D m_extents;
	};
	class CBCmdSetScissor final : public CBCmd
	{
	public:
		explicit CBCmdSetScissor(const platform::util::Extents2D extents) :
			m_extents(extents) { }

		void record(
			vk::CommandBuffer& cmd,
			std::vector<std::shared_ptr<render::Resource>>& resources
		) override;
	private:
		platform::util::Extents2D m_extents;
	};
	class CBCmdDraw final : public CBCmd
	{
	public:
		explicit CBCmdDraw(
			std::shared_ptr<render::RenderPipeline> pipeline,
			std::shared_ptr<render::VertexBuffer> vertexBuffer,
			std::shared_ptr<render::VertexBuffer> instanceBuffer
		) :
			m_pipeline(std::move(pipeline)),
			m_vertexBuffer(std::move(vertexBuffer)),
			m_instanceBuffer(std::move(instanceBuffer))
		{ }

		void record(
			vk::CommandBuffer& cmd,
			std::vector<std::shared_ptr<render::Resource>>& resources
		) override;
	private:
		std::shared_ptr<render::RenderPipeline> m_pipeline;
		std::shared_ptr<render::VertexBuffer> m_vertexBuffer;
		std::shared_ptr<render::VertexBuffer> m_instanceBuffer;
	};
	
	class CommandBuffer final : public render::CommandBuffer, public util::ScalableStagingResource
	{
	public:
		CommandBuffer(
			std::shared_ptr<VulkanContext> context,
			uint32_t stages
		);

		void tick();

		void reserve(uint32_t stages) override;
		
		void beginRecording(const std::shared_ptr<render::FramebufferFormat>& format) override;
		void setViewportAndScissor(std::shared_ptr<render::IRenderTarget> renderTarget) override;
		void setViewport(platform::util::Extents2D extents) override;
		void setScissor(platform::util::Extents2D extents) override;
		void draw(
			std::shared_ptr<render::RenderPipeline> pipeline,
			std::shared_ptr<render::VertexBuffer> vertexBuffer,
			std::shared_ptr<render::VertexBuffer> instanceBuffer
		) override;
		void finishRecording() override;

		[[nodiscard]] vk::CommandBuffer& get();

	private:
		std::shared_ptr<VulkanContext> m_context;

		std::vector<vk::UniqueCommandBuffer> m_commandBuffers;
		std::vector<std::vector<std::shared_ptr<Resource>>> m_resources;
		std::vector<std::unique_ptr<CBCmd>> m_commandQueue;
		uint32_t m_readIndex = 0;
		uint32_t m_leftoverWrites = 0;
	};
}
