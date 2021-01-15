#pragma once
#include <memory>

#include "texture.h"

namespace digbuild::platform::render
{
	class Framebuffer : public std::enable_shared_from_this<Framebuffer>
	{
	public:
		Framebuffer() = default;
		virtual ~Framebuffer() = default;
		Framebuffer(const Framebuffer& other) = delete;
		Framebuffer(Framebuffer&& other) noexcept = delete;
		Framebuffer& operator=(const Framebuffer& other) = delete;
		Framebuffer& operator=(Framebuffer&& other) noexcept = delete;

		[[nodiscard]] virtual uint32_t getWidth() const = 0;
		[[nodiscard]] virtual uint32_t getHeight() const = 0;

		[[nodiscard]] virtual std::vector<Texture*> getTextures() const = 0;
	};
}
