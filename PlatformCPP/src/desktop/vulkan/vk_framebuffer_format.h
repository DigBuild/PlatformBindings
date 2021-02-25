#pragma once
#include <vulkan.h>

#include "vk_context.h"
#include "../../render/framebuffer_format.h"
#include "../../render/render_context.h"

namespace digbuild::platform::desktop::vulkan
{
	class FramebufferFormat final : public render::FramebufferFormat
	{
	public:
		FramebufferFormat(
			std::shared_ptr<VulkanContext> context,
			std::vector<render::FramebufferAttachmentDescriptor> attachments,
			const std::vector<render::FramebufferRenderStageDescriptor>& renderStages
		);
		FramebufferFormat(
			std::shared_ptr<VulkanContext> context,
			vk::UniqueRenderPass renderPass,
			std::vector<render::FramebufferAttachmentDescriptor> attachments
		);

		[[nodiscard]] const vk::RenderPass& getPass() const
		{
			return *m_renderPass;
		}

		[[nodiscard]] uint32_t getAttachmentCount() const override
		{
			return static_cast<uint32_t>(m_attachments.size());
		}

		[[nodiscard]] const std::vector<render::FramebufferAttachmentDescriptor>& getAttachments() const
		{
			return m_attachments;
		}

	private:
		std::shared_ptr<VulkanContext> m_context;
		vk::UniqueRenderPass m_renderPass;
		std::vector<render::FramebufferAttachmentDescriptor> m_attachments;
	};
}
