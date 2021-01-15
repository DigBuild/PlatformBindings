#pragma once
#include <memory>

namespace digbuild::platform::render
{
	class FramebufferFormat : public std::enable_shared_from_this<FramebufferFormat>
	{
	public:
		FramebufferFormat() = default;
		virtual ~FramebufferFormat() = default;
		FramebufferFormat(const FramebufferFormat& other) = delete;
		FramebufferFormat(FramebufferFormat&& other) noexcept = delete;
		FramebufferFormat& operator=(const FramebufferFormat& other) = delete;
		FramebufferFormat& operator=(FramebufferFormat&& other) noexcept = delete;
	};
}
