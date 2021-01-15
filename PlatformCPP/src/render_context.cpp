#include "render_context.h"

namespace digbuild::platform
{
	struct FramebufferColorAttachmentDescriptorC
	{
		TextureFormat format;

		[[nodiscard]] FramebufferColorAttachmentDescriptor toCpp() const
		{
			return FramebufferColorAttachmentDescriptor{
				format
			};
		}
	};
	
	struct FramebufferDepthStencilAttachmentDescriptorC
	{
		TextureFormat format;

		[[nodiscard]] FramebufferDepthStencilAttachmentDescriptor toCpp() const
		{
			return FramebufferDepthStencilAttachmentDescriptor{
				format
			};
		}
	};
	
	struct FramebufferRenderStageDescriptorC
	{
		uint32_t attachmentCount;
		uint32_t* attachments;
		uint32_t dependencyCount;
		uint32_t* dependencies;

		[[nodiscard]] FramebufferRenderStageDescriptor toCpp() const
		{
			return FramebufferRenderStageDescriptor{
				std::vector<uint32_t>(attachments, attachments + attachmentCount),
				std::vector<uint32_t>(dependencies, dependencies + dependencyCount)
			};
		}
	};
}
