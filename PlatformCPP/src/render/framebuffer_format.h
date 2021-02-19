#pragma once
#include <memory>

#include "resource.h"

namespace digbuild::platform::render
{
	class FramebufferFormat : public Resource, public std::enable_shared_from_this<FramebufferFormat>
	{
	public:
		FramebufferFormat() = default;
		~FramebufferFormat() override = default;
		FramebufferFormat(const FramebufferFormat& other) = delete;
		FramebufferFormat(FramebufferFormat&& other) noexcept = delete;
		FramebufferFormat& operator=(const FramebufferFormat& other) = delete;
		FramebufferFormat& operator=(FramebufferFormat&& other) noexcept = delete;

		[[nodiscard]] virtual uint32_t getAttachmentCount() const = 0;
	};
}
