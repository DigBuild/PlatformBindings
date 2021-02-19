#pragma once
#include "resource.h"

namespace digbuild::platform::render
{
	class Framebuffer;
	
	class IRenderTarget : public Resource
	{
	public:
		IRenderTarget() = default;
		~IRenderTarget() override = default;
		IRenderTarget(const IRenderTarget& other) = delete;
		IRenderTarget(IRenderTarget&& other) noexcept = delete;
		IRenderTarget& operator=(const IRenderTarget& other) = delete;
		IRenderTarget& operator=(IRenderTarget&& other) noexcept = delete;

		[[nodiscard]] virtual Framebuffer& getFramebuffer() = 0;
	};
}
