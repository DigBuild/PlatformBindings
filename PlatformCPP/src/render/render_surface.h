#pragma once
#include <functional>

#include "render_context.h"

namespace digbuild::platform::render
{
	class RenderSurface;
	
	using RenderSurfaceUpdateFunction = std::function<void(const RenderSurface&, const RenderContext&)>;
	
	class RenderSurface : public std::enable_shared_from_this<RenderSurface>
	{
	public:
		RenderSurface() = default;
		virtual ~RenderSurface() = default;
		RenderSurface(const RenderSurface& other) = delete;
		RenderSurface(RenderSurface&& other) noexcept = delete;
		RenderSurface& operator=(const RenderSurface& other) = delete;
		RenderSurface& operator=(RenderSurface&& other) noexcept = delete;

		[[nodiscard]] virtual uint32_t getWidth() const = 0;
		[[nodiscard]] virtual uint32_t getHeight() const = 0;
		[[nodiscard]] virtual std::string getTitle() const = 0;
		[[nodiscard]] virtual bool isFullscreen() const = 0;
		[[nodiscard]] virtual bool isVisible() const = 0;

		virtual void setWidth(uint32_t width) { }
		virtual void setHeight(uint32_t height) { }
		virtual void setTitle(const std::string& title) { }
		virtual void setFullscreen(bool fullscreen) { }

		virtual void close() = 0;
		virtual void waitClosed() = 0;
	};
}
