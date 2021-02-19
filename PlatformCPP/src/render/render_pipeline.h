#pragma once
#include <memory>

#include "resource.h"

namespace digbuild::platform::render
{
	class RenderPipeline : public Resource, public std::enable_shared_from_this<RenderPipeline>
	{
	public:
		RenderPipeline() = default;
		~RenderPipeline() override = default;
		RenderPipeline(const RenderPipeline& other) = delete;
		RenderPipeline(RenderPipeline&& other) noexcept = delete;
		RenderPipeline& operator=(const RenderPipeline& other) = delete;
		RenderPipeline& operator=(RenderPipeline&& other) noexcept = delete;
	};
}
