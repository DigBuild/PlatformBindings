#pragma once
#include <memory>

namespace digbuild::platform::render
{
	class UniformBuffer
	{
		
	};
	class RenderPipeline : public std::enable_shared_from_this<RenderPipeline>
	{
	public:
		RenderPipeline() = default;
		virtual ~RenderPipeline() = default;
		RenderPipeline(const RenderPipeline& other) = delete;
		RenderPipeline(RenderPipeline&& other) noexcept = delete;
		RenderPipeline& operator=(const RenderPipeline& other) = delete;
		RenderPipeline& operator=(RenderPipeline&& other) noexcept = delete;

		[[nodiscard]] virtual std::vector<UniformBuffer*> getUniformBuffers() const = 0;
	};
}
