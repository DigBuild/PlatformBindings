#pragma once
#include <memory>
#include <vector>

#include "resource.h"

namespace digbuild::platform::render
{
	class UniformBuffer : public Resource, public std::enable_shared_from_this<UniformBuffer>
	{
	public:
		UniformBuffer() = default;
		~UniformBuffer() override = default;
		UniformBuffer(const UniformBuffer& other) = delete;
		UniformBuffer(UniformBuffer&& other) noexcept = delete;
		UniformBuffer& operator=(const UniformBuffer& other) = delete;
		UniformBuffer& operator=(UniformBuffer&& other) noexcept = delete;
		
		virtual void write(const std::vector<uint8_t>& data) = 0;
	};
}
