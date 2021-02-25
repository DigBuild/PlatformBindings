#include "vk_framebuffer_format.h"

#include <iterator>

namespace digbuild::platform::desktop::vulkan
{
	vk::AttachmentDescription toVulkan(const render::FramebufferAttachmentDescriptor& attachment)
	{
		vk::ImageLayout layout = {};
		switch (attachment.type)
		{
		case render::FramebufferAttachmentType::COLOR:
			layout = vk::ImageLayout::eColorAttachmentOptimal;
			break;
		case render::FramebufferAttachmentType::DEPTH_STENCIL:
			layout = vk::ImageLayout::eDepthStencilAttachmentOptimal;
			break;
		// default:
		// 	throw std::runtime_error("Invalid type.");
		}
		return vk::AttachmentDescription{
			{},
			util::toVulkanFormat(attachment.format),
			vk::SampleCountFlagBits::e1,
			vk::AttachmentLoadOp::eClear,
			vk::AttachmentStoreOp::eStore,
			vk::AttachmentLoadOp::eDontCare,
			vk::AttachmentStoreOp::eDontCare,
			vk::ImageLayout::eUndefined,
			layout
		};
	}

	FramebufferFormat::FramebufferFormat(
		std::shared_ptr<VulkanContext> context,
		std::vector<render::FramebufferAttachmentDescriptor> attachments,
		const std::vector<render::FramebufferRenderStageDescriptor>& renderStages
	) :
		m_context(std::move(context)),
		m_attachments(std::move(attachments))
	{
		std::vector<vk::AttachmentDescription> attachmentDescriptions;
		std::vector<vk::AttachmentReference> attachmentReferences;
		std::vector<uint32_t> allAttachments;
		attachmentDescriptions.reserve(m_attachments.size());
		attachmentReferences.reserve(m_attachments.size());
		for (const auto& attachment : m_attachments)
		{
			const auto i = static_cast<uint32_t>(attachmentDescriptions.size());
			const auto description = toVulkan(attachment);
			attachmentDescriptions.push_back(description);
			attachmentReferences.emplace_back(i, description.finalLayout);
			allAttachments.push_back(i);
		}

		std::vector<vk::SubpassDescription> subpassDescriptions;
		std::vector<vk::SubpassDependency> subpassDependencies;
		std::vector<std::vector<vk::AttachmentReference>> colorAttachments;
		std::vector<std::vector<uint32_t>> otherAttachments;
		colorAttachments.resize(renderStages.size());
		otherAttachments.resize(renderStages.size());
		
		auto stageID = 0u;
		for (const auto& renderStage : renderStages)
		{
			for (auto attachment : renderStage.colorAttachments)
				colorAttachments[stageID].push_back(attachmentReferences[attachment]);
				
			const auto* depthStencilAttachment =
				renderStage.depthStencilAttachment != UINT32_MAX ?
					&attachmentReferences[renderStage.depthStencilAttachment] :
					nullptr;

			auto& other = otherAttachments[stageID];
			std::set_difference(
				allAttachments.begin(), allAttachments.end(),
				renderStage.colorAttachments.begin(), renderStage.colorAttachments.end(),
				std::back_inserter(other)
			);
			
			other.erase(std::remove(other.begin(), other.end(), renderStage.depthStencilAttachment), other.end());
			
			subpassDescriptions.push_back({
				{}, vk::PipelineBindPoint::eGraphics,
				{},
				colorAttachments[stageID],
				{},
				depthStencilAttachment,
				otherAttachments[stageID]
			});

			subpassDependencies.push_back({
				VK_SUBPASS_EXTERNAL, stageID,
				vk::PipelineStageFlagBits::eColorAttachmentOutput | vk::PipelineStageFlagBits::eEarlyFragmentTests,
				vk::PipelineStageFlagBits::eColorAttachmentOutput | vk::PipelineStageFlagBits::eEarlyFragmentTests,
				{},
				vk::AccessFlagBits::eColorAttachmentWrite | vk::AccessFlagBits::eDepthStencilAttachmentWrite
			});
			for (auto dependency : renderStage.dependencies)
			{
				subpassDependencies.push_back({
					dependency, stageID,
					vk::PipelineStageFlagBits::eBottomOfPipe,
					vk::PipelineStageFlagBits::eTopOfPipe,
					{}, {}
				});
			}
			
			stageID++;
		}
		
		m_renderPass = m_context->m_device->createRenderPassUnique({
			{},
			attachmentDescriptions,
			subpassDescriptions,
			subpassDependencies
		});
	}

	FramebufferFormat::FramebufferFormat(
		std::shared_ptr<VulkanContext> context,
		vk::UniqueRenderPass renderPass,
		std::vector<render::FramebufferAttachmentDescriptor> attachments
	) :
		m_context(std::move(context)),
		m_renderPass(std::move(renderPass)),
		m_attachments(std::move(attachments))
	{
	}
}

