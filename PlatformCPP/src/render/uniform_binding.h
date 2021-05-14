#pragma once
#include <memory>

#include "resource.h"
#include "uniform_buffer.h"

namespace digbuild::platform::render
{
	class UniformBinding : public Resource, public std::enable_shared_from_this<UniformBinding>
	{
	public:
		UniformBinding() = default;
		~UniformBinding() override = default;
		UniformBinding(const UniformBinding& other) = delete;
		UniformBinding(UniformBinding&& other) noexcept = delete;
		UniformBinding& operator=(const UniformBinding& other) = delete;
		UniformBinding& operator=(UniformBinding&& other) noexcept = delete;

		virtual void update(
			std::shared_ptr<UniformBuffer> buffer
		) = 0;
	};
}
