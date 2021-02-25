#pragma once

#include "framebuffer_format.h"
#include "render_target.h"
#include "texture.h"

namespace digbuild::platform::render
{
	class Framebuffer : public IRenderTarget, public std::enable_shared_from_this<Framebuffer>
	{
	public:
		Framebuffer() = default;
		~Framebuffer() override = default;
		Framebuffer(const Framebuffer& other) = delete;
		Framebuffer(Framebuffer&& other) noexcept = delete;
		Framebuffer& operator=(const Framebuffer& other) = delete;
		Framebuffer& operator=(Framebuffer&& other) noexcept = delete;

		[[nodiscard]] virtual const FramebufferFormat& getFormat() const = 0;
		
		[[nodiscard]] virtual uint32_t getWidth() const = 0;
		[[nodiscard]] virtual uint32_t getHeight() const = 0;
		
		[[nodiscard]] virtual std::shared_ptr<Texture> getTexture(uint32_t attachment) = 0;

		[[nodiscard]] Framebuffer& getFramebuffer() override
		{
			return *this;
		}
	};
}
