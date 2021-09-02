#pragma once
#include <functional>

#include "../render/render_context.h"

namespace digbuild::platform::desktop
{
	class RenderContext : public render::RenderContext
	{
	public:
		RenderContext() = default;
		~RenderContext() override = default;
		RenderContext(const RenderContext& other) = delete;
		RenderContext(RenderContext&& other) noexcept = delete;
		RenderContext& operator=(const RenderContext& other) = delete;
		RenderContext& operator=(RenderContext&& other) noexcept = delete;

		virtual void updateFirst() = 0;
		virtual void updateLast() = 0;

		virtual render::Framebuffer& getFramebuffer() = 0;
	};

	class RenderSurface;
	using RenderContextFactory = std::function<std::unique_ptr<RenderContext>(RenderSurface&, RenderSurface*)>;
}
