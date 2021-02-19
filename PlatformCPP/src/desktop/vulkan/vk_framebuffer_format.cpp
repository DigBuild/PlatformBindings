#include "vk_framebuffer_format.h"

#include <iterator>

namespace digbuild::platform::desktop::vulkan
{
	vk::Format toVulkan(const render::TextureFormat format)
	{
		switch (format)
		{
			
		}
		throw std::runtime_error("Invalid type.");
	}
	
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
			toVulkan(attachment.format),
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
		const std::vector<render::FramebufferAttachmentDescriptor>& attachments,
		const std::vector<render::FramebufferRenderStageDescriptor>& renderStages
	) :
		m_context(std::move(context)),
		m_attachmentCount(static_cast<uint32_t>(attachments.size()))
	{
		std::vector<vk::AttachmentDescription> attachmentDescriptions;
		std::vector<vk::AttachmentReference> attachmentReferences;
		std::vector<uint32_t> allAttachments;
		attachmentDescriptions.reserve(attachments.size());
		attachmentReferences.reserve(attachments.size());
		for (const auto& attachment : attachments)
		{
			const auto i = static_cast<uint32_t>(attachmentDescriptions.size());
			const auto description = toVulkan(attachment);
			attachmentDescriptions.push_back(description);
			attachmentReferences.emplace_back(i, description.finalLayout);
			allAttachments.push_back(i);
		}

		std::vector<vk::SubpassDescription> subpassDescriptions;
		std::vector<vk::SubpassDependency> subpassDependencies;
		auto i = 0u;
		for (const auto& renderStage : renderStages)
		{	
			std::vector<vk::AttachmentReference> colorAttachments;
			for (auto attachment : renderStage.colorAttachments)
				colorAttachments.push_back(attachmentReferences[attachment]);
				
			const auto* depthStencilAttachment =
				renderStage.depthStencilAttachment != UINT32_MAX ?
					&attachmentReferences[renderStage.depthStencilAttachment] :
					nullptr;

			std::vector<uint32_t> otherAttachments;
			std::set_difference(
				renderStage.colorAttachments.begin(), renderStage.colorAttachments.end(),
				renderStage.colorAttachments.begin(), renderStage.colorAttachments.end(),
				std::back_inserter(otherAttachments)
			);
			
			subpassDescriptions.push_back({
				{}, vk::PipelineBindPoint::eGraphics,
				{},
				colorAttachments,
				{},
				depthStencilAttachment,
				otherAttachments
			});

			subpassDependencies.push_back({
				i, i,
				vk::PipelineStageFlagBits::eColorAttachmentOutput | vk::PipelineStageFlagBits::eEarlyFragmentTests,
				vk::PipelineStageFlagBits::eColorAttachmentOutput | vk::PipelineStageFlagBits::eEarlyFragmentTests,
				{},
				vk::AccessFlagBits::eColorAttachmentWrite | vk::AccessFlagBits::eDepthStencilAttachmentWrite
			});
			for (auto dependency : renderStage.dependencies)
			{
				subpassDependencies.push_back({
					dependency, i,
					vk::PipelineStageFlagBits::eBottomOfPipe,
					vk::PipelineStageFlagBits::eTopOfPipe,
					{}, {}
				});
			}
			
			i++;
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
		const uint32_t attachmentCount
	) :
		m_context(std::move(context)),
		m_renderPass(std::move(renderPass)),
		m_attachmentCount(attachmentCount)
	{
	}
}

